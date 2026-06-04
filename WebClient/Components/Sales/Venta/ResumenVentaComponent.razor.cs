using Microsoft.AspNetCore.Components;
using WebClient.Models.Sales;

namespace WebClient.Components.Sales.Venta;

public partial class ResumenVentaComponent
{
    [Parameter] public PuntoVentaViewModel PuntoVenta { get; set; } = new();
    [Parameter] public EventCallback<string> OnRemoveItem { get; set; }
    [Parameter] public EventCallback<string> OnIncreaseQuantity { get; set; }
    [Parameter] public EventCallback<string> OnDecreaseQuantity { get; set; }

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

    private static string FormatMoney(decimal amount)
    {
        return $"Bs {amount:N2}";
    }
}
