using Microsoft.AspNetCore.Components;
using System.Globalization;
using WebClient.Models.Sales;

namespace WebClient.Components.Sales.Venta;

public partial class ResumenVentaComponent
{
    [Parameter] public PuntoVentaViewModel PuntoVenta { get; set; } = new();
    [Parameter] public EventCallback<string> OnRemoveItem { get; set; }
    [Parameter] public EventCallback<string> OnIncreaseQuantity { get; set; }
    [Parameter] public EventCallback<string> OnDecreaseQuantity { get; set; }
    [Parameter] public EventCallback<QuantityChangeViewModel> OnQuantityChanged { get; set; }
    [Parameter] public EventCallback OnClearSale { get; set; }
    [Parameter] public EventCallback OnOpenPayment { get; set; }

    private Task RemoveItemAsync(string productId)
    {
        return OnRemoveItem.InvokeAsync(productId);
    }

    private Task IncreaseQuantityAsync(string productId)
    {
        return OnIncreaseQuantity.InvokeAsync(productId);
    }

    private Task DecreaseQuantityAsync(string productId)
    {
        return OnDecreaseQuantity.InvokeAsync(productId);
    }

    private Task SetQuantityAsync(ItemsViewModel item, decimal quantity)
    {
        item.Quantity = quantity;
        return OnQuantityChanged.InvokeAsync(new QuantityChangeViewModel
        {
            ProductId = item.ProductId,
            Quantity = quantity
        });
    }

    private Task ClearSaleAsync()
    {
        return OnClearSale.InvokeAsync();
    }

    private Task OpenPaymentAsync()
    {
        return OnOpenPayment.InvokeAsync();
    }

    private static string FormatMoney(decimal amount)
    {
        return $"Bs {amount:N2}";
    }

    private static string FormatQuantity(decimal quantity)
    {
        return quantity.ToString("0.##", CultureInfo.InvariantCulture);
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
}
