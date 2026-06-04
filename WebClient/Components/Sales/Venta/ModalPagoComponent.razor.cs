using Microsoft.AspNetCore.Components;
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
    private string PaymentNote { get; set; } = string.Empty;
    private decimal DiscountAmount { get; set; }
    private decimal ServiceCharge { get; set; }
    private decimal AmountReceived { get; set; }
    private bool IsNoteOpen { get; set; }
    private decimal SelectedSubtotal => PaymentItems.Where(item => item.IsSelected).Sum(item => item.Total);
    private decimal PaymentTotal => Math.Max(0, SelectedSubtotal - DiscountAmount + ServiceCharge);
    private decimal ChangeAmount => Math.Max(0, AmountReceived - PaymentTotal);

    protected override void OnParametersSet()
    {
        if (Visible && !_wasVisible)
        {
            LoadPaymentItems();
        }

        _wasVisible = Visible;
    }

    private void LoadPaymentItems()
    {
        PaymentNote = string.Empty;
        DiscountAmount = 0;
        ServiceCharge = 0;
        AmountReceived = 0;
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
    }

    private void DecreasePayQuantity(ModalPagoItemViewModel item)
    {
        if (!item.IsSelected || item.QuantityToPay <= 1)
        {
            return;
        }

        item.QuantityToPay -= 1;
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

        if (paidItems.Count == 0)
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
}
