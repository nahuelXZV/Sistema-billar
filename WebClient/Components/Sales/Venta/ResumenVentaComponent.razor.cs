using Microsoft.AspNetCore.Components;
using System.Globalization;
using WebClient.Models.Sales;

namespace WebClient.Components.Sales.Venta;

public partial class ResumenVentaComponent
{
    [Parameter] public PuntoVentaViewModel PuntoVenta { get; set; } = new();
    [Parameter] public EventCallback<ItemsViewModel> OnEliminarItem { get; set; }
    [Parameter] public EventCallback<ItemsViewModel> OnIncrementarCantidad { get; set; }
    [Parameter] public EventCallback<ItemsViewModel> OnDecrementarCantidad { get; set; }
    [Parameter] public EventCallback<CantidadModificada> OnCantidadModificada { get; set; }
    [Parameter] public EventCallback<ItemsViewModel> OnClienteModificado { get; set; }
    [Parameter] public EventCallback OnLimpiarVenta { get; set; }
    [Parameter] public EventCallback OnAbrirPago { get; set; }
    [Parameter] public EventCallback OnGuardar { get; set; }
    [Parameter] public bool MostrarSelectorCliente { get; set; }
    [Parameter] public bool MostrarGuardar { get; set; }
    [Parameter] public bool Guardando { get; set; }

    private bool MostrarConfirmacionEliminar { get; set; }
    private bool EliminandoItem { get; set; }
    private ItemsViewModel? ItemPendienteEliminacion { get; set; }

    private void SolicitarEliminacionItem(ItemsViewModel item)
    {
        ItemPendienteEliminacion = item;
        MostrarConfirmacionEliminar = true;
    }

    private void CancelarEliminacionItem()
    {
        if (EliminandoItem)
        {
            return;
        }

        ItemPendienteEliminacion = null;
        MostrarConfirmacionEliminar = false;
    }

    private async Task ConfirmarEliminacionItemAsync()
    {
        if (ItemPendienteEliminacion is null || EliminandoItem)
        {
            return;
        }

        EliminandoItem = true;

        try
        {
            await OnEliminarItem.InvokeAsync(ItemPendienteEliminacion);
            ItemPendienteEliminacion = null;
            MostrarConfirmacionEliminar = false;
        }
        finally
        {
            EliminandoItem = false;
        }
    }

    private Task IncrementarCantidad(ItemsViewModel item)
    {
        return OnIncrementarCantidad.InvokeAsync(item);
    }

    private Task DecrementarCantidad(ItemsViewModel item)
    {
        return OnDecrementarCantidad.InvokeAsync(item);
    }

    private Task SetearCantidad(ItemsViewModel item, decimal quantity)
    {
        item.Cantidad = quantity;
        return OnCantidadModificada.InvokeAsync(new CantidadModificada
        {
            ProductId = item.IdProducto,
            ProductConversionId = item.IdProductoConversion,
            IdCliente = item.IdCliente,
            EsTiempoMesa = item.EsTiempoMesa,
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

    private Task ClienteModificado(ItemsViewModel item)
    {
        if (!item.IdCliente.HasValue || item.IdCliente.Value <= 0)
        {
            return Task.CompletedTask;
        }

        return OnClienteModificado.InvokeAsync(item);
    }

    private Task Guardar()
    {
        return OnGuardar.InvokeAsync();
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
