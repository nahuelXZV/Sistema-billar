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

    private bool _wasVisible;
    private bool IsNoteOpen { get; set; }
    private List<ModalPagoItemViewModel> DetalleItems { get; set; } = [];
    private List<MetodoPagoDTO> MetodosPago { get; set; } = [];
    private List<DetallePagoViewModel> DetallePagos { get; set; } = [];
    private long SelectedMetodoPagoId { get; set; }
    private string NotaVenta { get; set; } = string.Empty;
    private decimal DescuentoGlobal { get; set; }
    private decimal RecargoGlobal { get; set; }
    private decimal PaymentMethodAmount { get; set; }
    private decimal SelectedSubtotal => DetalleItems.Where(item => item.IsSelected).Sum(item => item.Total);
    private decimal PaymentTotal => Math.Max(0, SelectedSubtotal - DescuentoGlobal + RecargoGlobal);
    private decimal TotalPaid => DetallePagos.Sum(payment => payment.Monto);
    private decimal RemainingAmount => Math.Max(0, PaymentTotal - TotalPaid);
    private decimal ChangeAmount => Math.Max(0, TotalPaid - PaymentTotal);
    private bool HasSelectedItems => DetalleItems.Any(item => item.IsSelected && item.CantidadPagar > 0);
    private bool CanAddPaymentMethod => HasSelectedItems && PaymentTotal > 0 && SelectedMetodoPagoId > 0 && PaymentMethodAmount > 0;
    private bool CanConfirmPayment => HasSelectedItems && PaymentTotal > 0 && TotalPaid >= PaymentTotal;

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
        NotaVenta = string.Empty;
        DescuentoGlobal = 0;
        RecargoGlobal = 0;
        SelectedMetodoPagoId = MetodosPago.FirstOrDefault()?.Id ?? 0;
        DetallePagos.Clear();
        IsNoteOpen = false;

        DetalleItems = PuntoVenta.DetalleItems.Select(item => new ModalPagoItemViewModel
        {
            ProductId = item.ProductId,
            Nombre = item.Nombre,
            CantidadDisponible = item.Cantidad,
            CantidadPagar = item.Cantidad,
            PrecioUnitario = item.PrecioUnitario,
            IsSelected = true
        }).ToList();

        PaymentMethodAmount = PaymentTotal;
    }

    private async Task LoadMetodosPagoAsync()
    {
        MetodosPago = await AppServices.MetodoPagoService.GetAll();
        SelectedMetodoPagoId = MetodosPago.FirstOrDefault()?.Id ?? 0;

        if (PaymentMethodAmount <= 0)
        {
            PaymentMethodAmount = PaymentTotal;
        }
    }

    private void IncrementarCantidad(ModalPagoItemViewModel item)
    {
        if (!item.IsSelected || item.CantidadPagar >= item.CantidadDisponible)
        {
            return;
        }

        item.CantidadPagar = Math.Min(item.CantidadDisponible, item.CantidadPagar + 1);
        item.CantidadPagar.Redondear();
        ResetSuggestedPaymentAmountIfEmpty();
    }

    private void DisminuirCantidad(ModalPagoItemViewModel item)
    {
        if (!item.IsSelected || item.CantidadPagar <= 0.01m)
        {
            return;
        }

        item.CantidadPagar = Math.Max(0.01m, item.CantidadPagar - 1);
        item.CantidadPagar.Redondear();
        ResetSuggestedPaymentAmountIfEmpty();
    }

    private void SetearCantidad(ModalPagoItemViewModel item, decimal quantity)
    {
        if (!item.IsSelected)
        {
            return;
        }

        item.CantidadPagar = Math.Clamp(quantity, 0.01m, item.CantidadDisponible);
        item.CantidadPagar.Redondear();
        ResetSuggestedPaymentAmountIfEmpty();
    }

    private void AgregarMetodoPago()
    {
        var metodoPago = MetodosPago.FirstOrDefault(metodo => metodo.Id == SelectedMetodoPagoId);
        if (metodoPago is null || PaymentMethodAmount <= 0)
        {
            return;
        }

        var existingPayment = DetallePagos.FirstOrDefault(payment => payment.IdMetodoPago == metodoPago.Id);
        if (existingPayment is not null)
        {
            existingPayment.Monto += PaymentMethodAmount;
        }
        else
        {
            DetallePagos.Add(new DetallePagoViewModel
            {
                Id = Guid.NewGuid().ToString("N"),
                IdMetodoPago = metodoPago.Id,
                Nombre = metodoPago.Nombre,
                Abreviatura = metodoPago.Abreviatura,
                Icono = metodoPago.Icono,
                Monto = PaymentMethodAmount
            });
        }

        PaymentMethodAmount = RemainingAmount > 0 ? RemainingAmount : 0;
    }

    private void EliminarMetodoPago(string id)
    {
        DetallePagos.RemoveAll(payment => payment.Id == id);
        ResetSuggestedPaymentAmountIfEmpty();
    }

    private async Task ConfirmarPago()
    {
        var paidItems = DetalleItems.Where(item => item.IsSelected && item.CantidadPagar > 0)
            .Select(item => new ItemsViewModel
            {
                ProductId = item.ProductId,
                Nombre = item.Nombre,
                Cantidad = Math.Min(item.CantidadPagar, item.CantidadDisponible),
                PrecioUnitario = item.PrecioUnitario
            }).ToList();

        if (paidItems.Count == 0 || !CanConfirmPayment)
        {
            return;
        }

        await OnPaymentConfirmed.InvokeAsync(paidItems);
        await CloseAsync();
    }


    #region Utils
    private void ResetSuggestedPaymentAmountIfEmpty()
    {
        if (DetallePagos.Count == 0)
        {
            PaymentMethodAmount = PaymentTotal;
        }
    }

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
