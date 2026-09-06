using Microsoft.AspNetCore.Components;
using Domain.DTOs.Sales;
using System.Globalization;
using WebClient.Models.Sales;
using WebClient.Extensions;

namespace WebClient.Components.Sales.Venta;

public partial class ModalPagoComponent
{
    [Parameter] public PuntoVentaViewModel PuntoVenta { get; set; } = new();
    [Parameter] public bool Visible { get; set; }
    [Parameter] public EventCallback<bool> VisibleChanged { get; set; }
    [Parameter] public EventCallback<IReadOnlyList<ItemsViewModel>> OnPaymentConfirmed { get; set; }
    [Parameter] public bool BloquearPagoTiempo { get; set; }
    [Parameter] public bool EsOrdenVenta { get; set; }

    private List<MetodoPagoDTO> MetodosPago { get; set; } = [];
    private bool _wasVisible;
    private bool IsSubmitting { get; set; }
    private bool IsNoteOpen { get; set; }
    private bool MostrarAlertaCliente { get; set; }

    private long SelectedMetodoPagoId { get; set; }
    private decimal MontoPagar { get; set; }

    private decimal MontoPendiente => RedondearMoneda(Math.Max(0, PuntoVenta.MontoTotal - PuntoVenta.TotalPagado));
    private bool HasSelectedItems => PuntoVenta.ProductosPagar.Any(item => item.IsSelected && item.CantidadPagar > 0);
    private bool CanAddPaymentMethod => HasSelectedItems && PuntoVenta.MontoTotal > 0 && SelectedMetodoPagoId > 0 && MontoPagar > 0;
    private bool CanConfirmPayment => !IsSubmitting && HasSelectedItems && PuntoVenta.MontoTotal > 0 && PuntoVenta.TotalPagado >= PuntoVenta.MontoTotal;

    protected override async Task OnParametersSetAsync()
    {
        if (Visible && !_wasVisible)
        {
            LoadPaymentItems();
            await LoadMetodosPagoAsync();
        }

        _wasVisible = Visible;
    }

    private void LoadPaymentItems()
    {
        SelectedMetodoPagoId = MetodosPago.FirstOrDefault()?.Id ?? 0;
        PuntoVenta.DetallePagos.Clear();
        IsNoteOpen = false;
        PuntoVenta.FinalizarOrdenVenta = false;

        PuntoVenta.ProductosPagar = PuntoVenta.DetalleItems.Select(item => new ProductosPagar
        {
            IdOrdenVentaDetalle = item.IdOrdenVentaDetalle,
            IdCliente = item.IdCliente,
            IdProducto = item.IdProducto,
            IdProductoConversion = item.IdProductoConversion,
            Nombre = item.Nombre,
            NombreUnidadMedida = item.NombreUnidadMedida,
            AbreviaturaUnidadMedida = item.AbreviaturaUnidadMedida,
            FactorConversion = item.FactorConversion,
            CantidadDisponible = item.Cantidad,
            CantidadPagar = item.Cantidad,
            PrecioUnitario = item.PrecioUnitario,
            EsTiempoMesa = item.EsTiempoMesa,
            IsSelected = !(BloquearPagoTiempo && item.EsTiempoMesa)
        }).ToList();

        MontoPagar = RedondearMoneda(PuntoVenta.MontoTotal);
    }

    private void CambiarSeleccion(ProductosPagar item, object? valor)
    {
        if (BloquearPagoTiempo && item.EsTiempoMesa)
        {
            item.IsSelected = false;
            return;
        }

        item.IsSelected = valor is bool seleccionado && seleccionado;
        ResetSuggestedPaymentAmountIfEmpty();
    }

    private string GetNombreCliente(long? idCliente)
    {
        return PuntoVenta.Clientes.FirstOrDefault(cliente => cliente.Id == idCliente)?.Nombre ?? "Sin cliente";
    }

    private async Task LoadMetodosPagoAsync()
    {
        MetodosPago = await AppServices.MetodoPagoService.GetAll();
        SelectedMetodoPagoId = MetodosPago.FirstOrDefault()?.Id ?? 0;

        if (MontoPagar <= 0)
        {
            MontoPagar = RedondearMoneda(PuntoVenta.MontoTotal);
        }
    }

    #region Cantidades Handlers
    private void IncrementarCantidad(ProductosPagar item)
    {
        if (!item.IsSelected || item.CantidadPagar >= item.CantidadDisponible)
        {
            return;
        }

        item.CantidadPagar = Math.Min(item.CantidadDisponible, item.CantidadPagar + 1).Redondear();
        ResetSuggestedPaymentAmountIfEmpty();
    }

    private void DisminuirCantidad(ProductosPagar item)
    {
        if (!item.IsSelected || item.CantidadPagar <= 0.01m)
        {
            return;
        }

        item.CantidadPagar = Math.Max(0.01m, item.CantidadPagar - 1).Redondear();
        ResetSuggestedPaymentAmountIfEmpty();
    }

    private void SetearCantidad(ProductosPagar item, decimal quantity)
    {
        if (!item.IsSelected)
        {
            return;
        }

        item.CantidadPagar = Math.Clamp(quantity, 0.01m, item.CantidadDisponible).Redondear();
        ResetSuggestedPaymentAmountIfEmpty();
    }
    #endregion

    #region Métodos de pago
    private void SeleccionarMetodoPago(long metodoPagoId)
    {
        SelectedMetodoPagoId = metodoPagoId;
    }

    private void AgregarMetodoPago()
    {
        var metodoPago = MetodosPago.FirstOrDefault(metodo => metodo.Id == SelectedMetodoPagoId);
        var montoPagar = RedondearMoneda(MontoPagar);
        if (metodoPago is null || montoPagar <= 0)
        {
            return;
        }

        var existingPayment = PuntoVenta.DetallePagos.FirstOrDefault(payment => payment.IdMetodoPago == metodoPago.Id);
        if (existingPayment is not null)
        {
            existingPayment.MontoTotal = montoPagar;
        }
        else
        {
            PuntoVenta.DetallePagos.Add(new DetallesPago
            {
                Id = Guid.NewGuid().ToString("N"),
                IdMetodoPago = metodoPago.Id,
                Nombre = metodoPago.Nombre,
                Abreviatura = metodoPago.Abreviatura,
                Icono = metodoPago.Icono,
                MontoTotal = montoPagar,
            });
        }

        MontoPagar = MontoPendiente > 0 ? MontoPendiente : 0;
    }

    private void EliminarMetodoPago(string id)
    {
        PuntoVenta.DetallePagos.RemoveAll(payment => payment.Id == id);
        ResetSuggestedPaymentAmountIfEmpty();
    }

    private async Task ConfirmarPago()
    {
        if (IsSubmitting)
        {
            return;
        }

        if (PuntoVenta.ClienteSeleccionado == null)
        {
            MostrarAlertaCliente = true;
            return;
        }

        MostrarAlertaCliente = false;
        var paidItems = PuntoVenta.ProductosPagar.Where(item => item.IsSelected && item.CantidadPagar > 0)
            .Select(item => new ItemsViewModel
            {
                IdOrdenVentaDetalle = item.IdOrdenVentaDetalle,
                IdCliente = item.IdCliente,
                IdProducto = item.IdProducto,
                IdProductoConversion = item.IdProductoConversion,
                Nombre = item.Nombre,
                NombreUnidadMedida = item.NombreUnidadMedida,
                AbreviaturaUnidadMedida = item.AbreviaturaUnidadMedida,
                FactorConversion = item.FactorConversion,
                Cantidad = Math.Min(item.CantidadPagar, item.CantidadDisponible),
                PrecioUnitario = item.PrecioUnitario,
                EsTiempoMesa = item.EsTiempoMesa
            }).ToList();

        if (paidItems.Count == 0 || !CanConfirmPayment)
        {
            return;
        }

        IsSubmitting = true;
        try
        {
            await OnPaymentConfirmed.InvokeAsync(paidItems);
            await CloseAsync();
        }
        catch
        {
            // El componente padre muestra el error y la clave se conserva para reintentar.
        }
        finally
        {
            IsSubmitting = false;
        }
    }
    #endregion

    #region Utils
    private void ResetSuggestedPaymentAmountIfEmpty()
    {
        if (PuntoVenta.DetallePagos.Count == 0)
        {
            MontoPagar = RedondearMoneda(PuntoVenta.MontoTotal);
        }
    }

    private void SetearMontoPagar(ChangeEventArgs args)
    {
        MontoPagar = RedondearMoneda(ParseQuantity(args.Value));
    }

    private static decimal RedondearMoneda(decimal monto) =>
        Math.Round(monto, 2, MidpointRounding.AwayFromZero);

    private void ToggleNote()
    {
        IsNoteOpen = !IsNoteOpen;
    }
    private async Task CloseAsync()

    {
        Visible = false;
        _wasVisible = false;
        await VisibleChanged.InvokeAsync(false);
    }
    private async Task HandleVisibleChangedAsync(bool visible)
    {
        if (IsSubmitting)
        {
            return;
        }

        if (visible)
        {
            Visible = true;
            await VisibleChanged.InvokeAsync(true);
            return;
        }

        await CloseAsync();
    }
    private static decimal ParseQuantity(object? value)
    {
        var text = Convert.ToString(value, CultureInfo.CurrentCulture)?.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        var normalizedText = text.Replace(',', '.');
        if (decimal.TryParse(normalizedText, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var normalizedQuantity))
        {
            return normalizedQuantity;
        }

        return decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out var currentCultureQuantity)
            ? currentCultureQuantity
            : 0;
    }
    #endregion
}
