using Microsoft.AspNetCore.Components;
using Domain.DTOs.Sales;
using WebClient.Models.Sales;

namespace WebClient.Components.Sales.Venta;

public partial class ModalPagoComponent
{
    [Parameter] public PuntoVentaViewModel PuntoVenta { get; set; } = new();
    [Parameter] public bool Visible { get; set; }
    [Parameter] public EventCallback<bool> VisibleChanged { get; set; }
    [Parameter] public EventCallback<IReadOnlyList<PagoItemViewModel>> OnPaymentConfirmed { get; set; }

    private bool _wasVisible;
    private List<ModalPagoItemViewModel> PaymentItems { get; set; } = [];
    private List<MetodoPagoDTO> MetodosPago { get; set; } = [];
    private List<ModalMetodoPagoViewModel> PaymentMethods { get; set; } = [];
    private string PaymentNote { get; set; } = string.Empty;
    private decimal DiscountAmount { get; set; }
    private decimal ServiceCharge { get; set; }
    private long SelectedMetodoPagoId { get; set; }
    private decimal PaymentMethodAmount { get; set; }
    private bool IsNoteOpen { get; set; }
    private decimal SelectedSubtotal => PaymentItems.Where(item => item.IsSelected).Sum(item => item.Total);
    private decimal PaymentTotal => Math.Max(0, SelectedSubtotal - DiscountAmount + ServiceCharge);
    private decimal TotalPaid => PaymentMethods.Sum(payment => payment.Amount);
    private decimal RemainingAmount => Math.Max(0, PaymentTotal - TotalPaid);
    private decimal ChangeAmount => Math.Max(0, TotalPaid - PaymentTotal);
    private bool HasSelectedItems => PaymentItems.Any(item => item.IsSelected && item.QuantityToPay > 0);
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
        PaymentNote = string.Empty;
        DiscountAmount = 0;
        ServiceCharge = 0;
        SelectedMetodoPagoId = MetodosPago.FirstOrDefault()?.Id ?? 0;
        PaymentMethods.Clear();
        IsNoteOpen = false;

        PaymentItems = PuntoVenta.OrderItems
            .Select(item => new ModalPagoItemViewModel
            {
                ProductId = item.ProductId,
                Name = item.Name,
                AvailableQuantity = item.Quantity,
                QuantityToPay = item.Quantity,
                UnitPrice = item.UnitPrice,
                IsSelected = true
            })
            .ToList();

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

    private void ToggleNote()
    {
        IsNoteOpen = !IsNoteOpen;
    }

    private void IncreasePayQuantity(ModalPagoItemViewModel item)
    {
        if (!item.IsSelected || item.QuantityToPay >= item.AvailableQuantity)
        {
            return;
        }

        item.QuantityToPay += 1;
        ResetSuggestedPaymentAmountIfEmpty();
    }

    private void DecreasePayQuantity(ModalPagoItemViewModel item)
    {
        if (!item.IsSelected || item.QuantityToPay <= 1)
        {
            return;
        }

        item.QuantityToPay -= 1;
        ResetSuggestedPaymentAmountIfEmpty();
    }

    private void AddPaymentMethod()
    {
        var metodoPago = MetodosPago.FirstOrDefault(metodo => metodo.Id == SelectedMetodoPagoId);
        if (metodoPago is null || PaymentMethodAmount <= 0)
        {
            return;
        }

        var existingPayment = PaymentMethods.FirstOrDefault(payment => payment.IdMetodoPago == metodoPago.Id);
        if (existingPayment is not null)
        {
            existingPayment.Amount += PaymentMethodAmount;
        }
        else
        {
            PaymentMethods.Add(new ModalMetodoPagoViewModel
            {
                Id = Guid.NewGuid().ToString("N"),
                IdMetodoPago = metodoPago.Id,
                Name = metodoPago.Nombre,
                Abbreviation = metodoPago.Abreviatura,
                Icono = metodoPago.Icono,
                Amount = PaymentMethodAmount
            });
        }

        PaymentMethodAmount = RemainingAmount > 0 ? RemainingAmount : 0;
    }

    private void RemovePaymentMethod(string id)
    {
        PaymentMethods.RemoveAll(payment => payment.Id == id);
        ResetSuggestedPaymentAmountIfEmpty();
    }

    private void ResetSuggestedPaymentAmountIfEmpty()
    {
        if (PaymentMethods.Count == 0)
        {
            PaymentMethodAmount = PaymentTotal;
        }
    }

    private async Task ConfirmPaymentAsync()
    {
        var paidItems = PaymentItems
            .Where(item => item.IsSelected && item.QuantityToPay > 0)
            .Select(item => new PagoItemViewModel
            {
                ProductId = item.ProductId,
                Name = item.Name,
                Quantity = Math.Min(item.QuantityToPay, item.AvailableQuantity),
                UnitPrice = item.UnitPrice
            })
            .ToList();

        if (paidItems.Count == 0 || !CanConfirmPayment)
        {
            return;
        }

        await OnPaymentConfirmed.InvokeAsync(paidItems);
        await CloseAsync();
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

    private static string FormatMoney(decimal amount)
    {
        return $"Bs {amount:N2}";
    }

    private sealed class ModalPagoItemViewModel
    {
        public string ProductId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int AvailableQuantity { get; set; }
        public int QuantityToPay { get; set; }
        public decimal UnitPrice { get; set; }
        public bool IsSelected { get; set; }
        public decimal Total => QuantityToPay * UnitPrice;
    }

    private sealed class ModalMetodoPagoViewModel
    {
        public string Id { get; set; } = string.Empty;
        public long IdMetodoPago { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Abbreviation { get; set; } = string.Empty;
        public string Icono { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
