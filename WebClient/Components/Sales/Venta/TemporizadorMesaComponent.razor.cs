using Domain.DTOs.Configuration;
using Domain.DTOs.Inventory;
using Microsoft.AspNetCore.Components;
using System.Diagnostics;
using WebClient.Models.Sales;

namespace WebClient.Components.Sales.Venta;

public partial class TemporizadorMesaComponent : IAsyncDisposable
{
    [Parameter, EditorRequired] public MesaDTO Mesa { get; set; } = new();
    [Parameter, EditorRequired] public VentaViewModel Venta { get; set; } = new();
    [Parameter] public EventCallback OnTiempoFinalizado { get; set; }

    private readonly Stopwatch _stopwatch = new();
    private CancellationTokenSource? _timerCancellation;
    private ProductoDTO? TimedProduct { get; set; }
    private string TimerMessage { get; set; } = string.Empty;
    private bool IsTimerFinished { get; set; }
    private long _configuredMesaId;

    private bool IsTimerRunning => _stopwatch.IsRunning;
    private string ElapsedTimeText => FormatElapsedTime(_stopwatch.Elapsed);
    private decimal CurrentTimeAmount => TimedProduct is null
        ? 0
        : Math.Round((decimal)_stopwatch.Elapsed.TotalHours * TimedProduct.Precio, 2, MidpointRounding.AwayFromZero);

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        if (_configuredMesaId == Mesa.Id)
        {
            return;
        }

        ResetTimer();
        _configuredMesaId = Mesa.Id;
        await ConfigureTimerAsync();
    }

    private async Task ConfigureTimerAsync()
    {
        var tipoMesa = Mesa.TipoMesa;
        if (tipoMesa?.CobroPorTiempo != true || tipoMesa.IdProducto is null)
        {
            return;
        }

        var idVendedor = Venta.Vendedor?.Id ?? 0;
        if (idVendedor <= 0)
        {
            TimerMessage = "No se encontro un vendedor para calcular la tarifa.";
            return;
        }

        var producto = await AppServices.ProductoService.GetById(tipoMesa.IdProducto.Value, idVendedor);
        if (!producto.Activo)
        {
            TimerMessage = "El producto configurado para esta mesa esta inactivo.";
            return;
        }

        if (producto.Precio <= 0)
        {
            TimerMessage = "El producto de la mesa no tiene precio para este vendedor.";
            return;
        }

        TimedProduct = producto;
        StartTimer();
    }

    private void StartTimer()
    {
        _timerCancellation = new CancellationTokenSource();
        _stopwatch.Restart();
        IsTimerFinished = false;
        _ = RefreshTimerAsync(_timerCancellation.Token);
    }

    private async Task RefreshTimerAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void ToggleTimer()
    {
        if (IsTimerFinished || TimedProduct is null)
        {
            return;
        }

        if (_stopwatch.IsRunning)
        {
            _stopwatch.Stop();
            return;
        }

        _stopwatch.Start();
    }

    private async Task FinishTimerAsync()
    {
        if (TimedProduct is null || IsTimerFinished)
        {
            return;
        }

        if (Venta.PuntoVenta is null)
        {
            TimerMessage = "La venta aun esta cargando.";
            return;
        }

        _stopwatch.Stop();
        IsTimerFinished = true;
        _timerCancellation?.Cancel();

        var billedHours = Math.Round(
            (decimal)_stopwatch.Elapsed.TotalHours,
            2,
            MidpointRounding.AwayFromZero);

        billedHours = Math.Max(0.01m, billedHours);

        var existingItem = Venta.PuntoVenta.DetalleItems
            .FirstOrDefault(item => item.IdProducto == TimedProduct.Id);

        if (existingItem is null)
        {
            Venta.PuntoVenta.DetalleItems.Add(new ItemsViewModel
            {
                IdProducto = TimedProduct.Id,
                Nombre = $"{TimedProduct.Nombre} ({ElapsedTimeText})",
                Cantidad = billedHours,
                PrecioUnitario = TimedProduct.Precio
            });
        }
        else
        {
            existingItem.Nombre = $"{TimedProduct.Nombre} ({ElapsedTimeText})";
            existingItem.Cantidad = Math.Round(
                existingItem.Cantidad + billedHours,
                2,
                MidpointRounding.AwayFromZero);
            existingItem.PrecioUnitario = TimedProduct.Precio;
        }

        TimerMessage = $"{billedHours:0.00} hrs agregadas a la venta.";
        await OnTiempoFinalizado.InvokeAsync();
    }

    private void ResetTimer()
    {
        _timerCancellation?.Cancel();
        _timerCancellation?.Dispose();
        _timerCancellation = null;
        _stopwatch.Reset();
        TimedProduct = null;
        TimerMessage = string.Empty;
        IsTimerFinished = false;
    }

    private static string FormatElapsedTime(TimeSpan elapsed)
    {
        return $"{(int)elapsed.TotalHours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}";
    }

    private static string FormatMoney(decimal amount)
    {
        return $"Bs {amount:N2}";
    }

    public ValueTask DisposeAsync()
    {
        ResetTimer();
        return ValueTask.CompletedTask;
    }
}

