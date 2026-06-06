using Microsoft.AspNetCore.Components;
using System.Globalization;
using WebClient.Models.Sales;

namespace WebClient.Components.Sales.Venta;

public partial class ResumenVentaComponent
{
    [Parameter] public PuntoVentaViewModel PuntoVenta { get; set; } = new();
    [Parameter] public EventCallback<long> OnEliminarItem { get; set; }
    [Parameter] public EventCallback<long> OnIncrementarCantidad { get; set; }
    [Parameter] public EventCallback<long> OnDecrementarCantidad { get; set; }
    [Parameter] public EventCallback<CantidadModificada> OnCantidadModificada { get; set; }
    [Parameter] public EventCallback OnLimpiarVenta { get; set; }
    [Parameter] public EventCallback OnAbrirPago { get; set; }

    private Task EliminarItem(long productId)
    {
        return OnEliminarItem.InvokeAsync(productId);
    }

    private Task IncrementarCantidad(long productId)
    {
        return OnIncrementarCantidad.InvokeAsync(productId);
    }

    private Task DecrementarCantidad(long productId)
    {
        return OnDecrementarCantidad.InvokeAsync(productId);
    }

    private Task SetearCantidad(ItemsViewModel item, decimal quantity)
    {
        item.Cantidad = quantity;
        return OnCantidadModificada.InvokeAsync(new CantidadModificada
        {
            ProductId = item.ProductId,
            Cantidad = quantity
        });
    }

    private Task LimpiarVenta()
    {
        return OnLimpiarVenta.InvokeAsync();
    }

    private Task AbrirModalPago()
    {
        return OnAbrirPago.InvokeAsync();
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
