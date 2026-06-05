using Microsoft.AspNetCore.Components;
using WebClient.Models.Sales;

namespace WebClient.Components.Sales.Venta;

public partial class VentaComponent
{
    [Parameter] public VentaViewModel Model { get; set; } = new();
    private bool IsPaymentModalOpen { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Model.PuntoVenta ??= await LoadPuntoVentaAsync();
    }

    private async Task<PuntoVentaViewModel> LoadPuntoVentaAsync()
    {
        var categoriasBase = await AppServices.CategoriaService.GetCategoriasBase();
        return PuntoVentaMapper.Create(categoriasBase, Model.Vendedor);
    }

    private void AddProduct(ProductosViewModel product)
    {
        if (Model.PuntoVenta is null)
        {
            return;
        }

        var existingItem = Model.PuntoVenta.OrderItems.FirstOrDefault(item => item.ProductId == product.Id);
        if (existingItem is not null)
        {
            existingItem.Quantity = NormalizeQuantity(existingItem.Quantity + 1);
            return;
        }

        Model.PuntoVenta.OrderItems.Add(CreateOrderItem(product));
    }

    private void RemoveItem(string productId)
    {
        if (Model.PuntoVenta is null)
        {
            return;
        }

        var item = Model.PuntoVenta.OrderItems.FirstOrDefault(orderItem => orderItem.ProductId == productId);
        if (item is not null)
        {
            Model.PuntoVenta.OrderItems.Remove(item);
        }
    }

    private void IncreaseQuantity(string productId)
    {
        ChangeQuantity(productId, 1);
    }

    private void DecreaseQuantity(string productId)
    {
        ChangeQuantity(productId, -1);
    }

    private void SetQuantity(QuantityChangeViewModel quantityChange)
    {
        if (Model.PuntoVenta is null)
        {
            return;
        }

        var item = Model.PuntoVenta.OrderItems.FirstOrDefault(orderItem => orderItem.ProductId == quantityChange.ProductId);
        if (item is null)
        {
            return;
        }

        item.Quantity = NormalizeQuantity(quantityChange.Quantity);
        if (item.Quantity <= 0)
        {
            Model.PuntoVenta.OrderItems.Remove(item);
        }
    }

    private void ClearSale()
    {
        if (Model.PuntoVenta is null)
        {
            return;
        }

        Model.PuntoVenta.OrderItems.Clear();
        Model.PuntoVenta.SelectedPath.Clear();
        Model.PuntoVenta.CurrentNode = null;
        Model.PuntoVenta.ClienteSeleccionado = null;
        Model.PuntoVenta.NotaVenta = string.Empty;
        Model.PuntoVenta.DiscountAmount = 0;
        Model.PuntoVenta.ServiceCharge = 0;
    }

    private void OpenPaymentModal()
    {
        if (Model.PuntoVenta?.OrderItems.Count > 0)
        {
            IsPaymentModalOpen = true;
        }
    }

    private void ApplyPayment(IReadOnlyList<PagoItemViewModel> paidItems)
    {
        if (Model.PuntoVenta is null)
        {
            return;
        }

        foreach (var paidItem in paidItems)
        {
            var orderItem = Model.PuntoVenta.OrderItems.FirstOrDefault(item => item.ProductId == paidItem.ProductId);
            if (orderItem is null)
            {
                continue;
            }

            orderItem.Quantity = NormalizeQuantity(orderItem.Quantity - paidItem.Quantity);
            if (orderItem.Quantity <= 0)
            {
                Model.PuntoVenta.OrderItems.Remove(orderItem);
            }
        }
    }

    private void ChangeQuantity(string productId, decimal delta)
    {
        if (Model.PuntoVenta is null)
        {
            return;
        }

        var item = Model.PuntoVenta.OrderItems.FirstOrDefault(orderItem => orderItem.ProductId == productId);
        if (item is null)
        {
            return;
        }

        item.Quantity = NormalizeQuantity(item.Quantity + delta);
        if (item.Quantity <= 0)
        {
            Model.PuntoVenta.OrderItems.Remove(item);
        }
    }

    private static ItemsViewModel CreateOrderItem(ProductosViewModel product)
    {
        return new ItemsViewModel
        {
            ProductId = product.Id,
            Name = product.Name,
            Detail = product.CategoryLabel,
            Quantity = 1,
            UnitPrice = product.Price,
            IconCss = product.IconCss,
            ToneClass = product.ToneClass
        };
    }

    private static decimal NormalizeQuantity(decimal quantity)
    {
        return Math.Round(quantity, 2, MidpointRounding.AwayFromZero);
    }
}
