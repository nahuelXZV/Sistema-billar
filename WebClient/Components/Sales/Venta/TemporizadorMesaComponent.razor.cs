using Domain.DTOs.Configuration;
using Domain.DTOs.Inventory;
using Domain.DTOs.Sales;
using Microsoft.AspNetCore.Components;
using WebClient.Models.Sales;
using static Domain.Constants.Constantes;

namespace WebClient.Components.Sales.Venta;

public partial class TemporizadorMesaComponent : IAsyncDisposable
{
    [Parameter, EditorRequired] public MesaDTO Mesa { get; set; } = new();
    [Parameter, EditorRequired] public VentaViewModel Venta { get; set; } = new();
    [Parameter] public EventCallback OnGuardarOrden { get; set; }
    [Parameter] public EventCallback<OrdenMesaDTO> OnOrdenActualizada { get; set; }

    private CancellationTokenSource? _cancelacionTemporizador;
    private ProductoDTO? ProductoTiempo { get; set; }
    private string MensajeTemporizador { get; set; } = string.Empty;
    private long _idMesaConfigurada;
    private DateTime _ahora = DateTime.Now;
    private bool Iniciando { get; set; }
    private bool Finalizando { get; set; }
    private bool CronometroPendiente => Venta.OrdenMesa?.EstadoUsoMesa == (short)EstadoUsoMesa.Pendiente;
    private bool CronometroIniciado => Venta.OrdenMesa?.EstadoUsoMesa == (short)EstadoUsoMesa.EnCurso;

    private TimeSpan TiempoTranscurrido
    {
        get
        {
            var ordenMesa = Venta.OrdenMesa;
            if (ordenMesa is null || ordenMesa.EstadoUsoMesa == (short)EstadoUsoMesa.Pendiente)
            {
                return TimeSpan.Zero;
            }

            if (ordenMesa.EstadoUsoMesa == (short)EstadoUsoMesa.EnCurso)
            {
                return _ahora > ordenMesa.FechaInicio
                    ? _ahora - ordenMesa.FechaInicio
                    : TimeSpan.Zero;
            }

            return TimeSpan.FromMinutes((double)Math.Max(0, ordenMesa.MinutosConsumidos));
        }
    }

    private decimal ImporteTiempoActual => ProductoTiempo is null
        ? 0 : Math.Round((decimal)TiempoTranscurrido.TotalHours * ProductoTiempo.Precio, 2, MidpointRounding.AwayFromZero);

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        if (_idMesaConfigurada != Mesa.Id)
        {
            ReiniciarComponente();
            _idMesaConfigurada = Mesa.Id;
            await ConfigurarTemporizadorAsync();
        }

        if (CronometroIniciado)
        {
            IniciarActualizacionVisual();
            SincronizarDetalleTiempo();
        }
    }

    private async Task ConfigurarTemporizadorAsync()
    {
        var tipoMesa = Mesa.TipoMesa;
        if (tipoMesa?.CobroPorTiempo != true || tipoMesa.IdProducto is null)
        {
            return;
        }

        var idVendedor = Venta.Vendedor?.Id ?? 0;
        if (idVendedor <= 0)
        {
            MensajeTemporizador = "No se encontró un vendedor para calcular la tarifa.";
            return;
        }

        var producto = await AppServices.ProductoService.GetById(tipoMesa.IdProducto.Value, idVendedor);
        if (!producto.Activo)
        {
            MensajeTemporizador = "El producto configurado para esta mesa está inactivo.";
            return;
        }

        if (producto.Precio <= 0)
        {
            MensajeTemporizador = "El producto de la mesa no tiene precio para este vendedor.";
            return;
        }

        ProductoTiempo = producto;
        Venta.OrdenMesa ??= new OrdenMesaDTO();
        Venta.OrdenMesa.TarifaAplicada = producto.Precio;
    }

    private async Task IniciarCronometroAsync()
    {
        if (ProductoTiempo is null || CronometroIniciado || Iniciando)
        {
            return;
        }

        Iniciando = true;
        try
        {
            if (OnGuardarOrden.HasDelegate)
            {
                await OnGuardarOrden.InvokeAsync();
            }

            var idOrdenVenta = Venta.OrdenMesa?.Id ?? 0;
            if (idOrdenVenta <= 0)
            {
                throw new InvalidOperationException("Primero debe guardarse la orden de mesa.");
            }

            var ordenActualizada = await AppServices.OrdenMesaService.IniciarCronometro(idOrdenVenta);
            ordenActualizada.TarifaAplicada = ProductoTiempo.Precio;
            Venta.OrdenMesa = ordenActualizada;
            Venta.PuntoVenta.IdOrdenVenta = ordenActualizada.Id;
            Venta.PuntoVenta.IdUsoMesa = ordenActualizada.IdUsoMesa;
            _ahora = DateTime.Now;

            SincronizarDetalleTiempo();
            IniciarActualizacionVisual();

            if (OnOrdenActualizada.HasDelegate)
            {
                await OnOrdenActualizada.InvokeAsync(ordenActualizada);
            }
        }
        catch (Exception ex)
        {
            await ShowErrorMessage(ex);
        }
        finally
        {
            Iniciando = false;
        }
    }

    private async Task FinalizarCronometroAsync()
    {
        if (!CronometroIniciado || Finalizando)
        {
            return;
        }

        Finalizando = true;
        try
        {
            _ahora = DateTime.Now;
            SincronizarDetalleTiempo();

            if (OnGuardarOrden.HasDelegate)
            {
                await OnGuardarOrden.InvokeAsync();
            }

            var idOrdenVenta = Venta.OrdenMesa?.Id ?? 0;
            if (idOrdenVenta <= 0)
            {
                throw new InvalidOperationException("Primero debe guardarse la orden de mesa.");
            }

            var ordenActualizada = await AppServices.OrdenMesaService.FinalizarCronometro(idOrdenVenta);
            Venta.OrdenMesa = ordenActualizada;
            Venta.PuntoVenta.IdOrdenVenta = ordenActualizada.Id;
            Venta.PuntoVenta.IdUsoMesa = ordenActualizada.IdUsoMesa;
            DetenerActualizacionVisual();

            if (OnOrdenActualizada.HasDelegate)
            {
                await OnOrdenActualizada.InvokeAsync(ordenActualizada);
            }
        }
        catch (Exception ex)
        {
            await ShowErrorMessage(ex);
        }
        finally
        {
            Finalizando = false;
        }
    }

    private void IniciarActualizacionVisual()
    {
        if (_cancelacionTemporizador is not null)
        {
            return;
        }

        _cancelacionTemporizador = new CancellationTokenSource();
        _ = ActualizarTemporizadorAsync(_cancelacionTemporizador.Token);
    }

    private async Task ActualizarTemporizadorAsync(CancellationToken tokenCancelacion)
    {
        using var temporizador = new PeriodicTimer(TimeSpan.FromSeconds(1));

        try
        {
            while (await temporizador.WaitForNextTickAsync(tokenCancelacion))
            {
                _ahora = DateTime.Now;
                SincronizarDetalleTiempo();
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void SincronizarDetalleTiempo()
    {
        if (!CronometroIniciado || ProductoTiempo is null || Venta.PagoEnProceso)
        {
            return;
        }

        var horasFacturadas = Math.Max(0.01m, Math.Round((decimal)TiempoTranscurrido.TotalHours, 2, MidpointRounding.AwayFromZero));

        var detalleTiempo = Venta.PuntoVenta.DetalleItems.FirstOrDefault(detalle => detalle.EsTiempoMesa);

        if (detalleTiempo is null)
        {
            Venta.PuntoVenta.DetalleItems.Add(new ItemsViewModel
            {
                IdProducto = ProductoTiempo.Id,
                Nombre = ProductoTiempo.Nombre,
                Cantidad = horasFacturadas,
                PrecioUnitario = ProductoTiempo.Precio,
                EsTiempoMesa = true
            });
            return;
        }

        detalleTiempo.IdProducto = ProductoTiempo.Id;
        detalleTiempo.Nombre = ProductoTiempo.Nombre;
        detalleTiempo.Cantidad = horasFacturadas;
        detalleTiempo.PrecioUnitario = ProductoTiempo.Precio;
    }

    private void DetenerActualizacionVisual()
    {
        _cancelacionTemporizador?.Cancel();
        _cancelacionTemporizador?.Dispose();
        _cancelacionTemporizador = null;
    }

    private void ReiniciarComponente()
    {
        DetenerActualizacionVisual();
        ProductoTiempo = null;
        MensajeTemporizador = string.Empty;
        _ahora = DateTime.Now;
        Iniciando = false;
        Finalizando = false;
    }

    public ValueTask DisposeAsync()
    {
        ReiniciarComponente();
        return ValueTask.CompletedTask;
    }
}
