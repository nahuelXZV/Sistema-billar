using Microsoft.AspNetCore.Components;
using WebClient.Models.Sales;

namespace WebClient.Components.Sales.Venta;

public partial class VentaComponent
{
    [Parameter] public VentaViewModel Model { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Model.PuntoVenta ??= await LoadPuntoVentaAsync();
    }

    private async Task<PuntoVentaViewModel> LoadPuntoVentaAsync()
    {
        var categoriasBase = await AppServices.CategoriaService.GetCategoriasBase();
        return PuntoVentaMapper.Create(categoriasBase);
    }

    private void AddProduct(PuntoVentaProductoViewModel product)
    {
        if (Model.PuntoVenta is null)
        {
            return;
        }

        var existingItem = Model.PuntoVenta.OrderItems.FirstOrDefault(item => item.ProductId == product.Id);
        if (existingItem is not null)
        {
            existingItem.Quantity += 1;
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

    private void ChangeQuantity(string productId, int delta)
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

        item.Quantity += delta;
        if (item.Quantity <= 0)
        {
            Model.PuntoVenta.OrderItems.Remove(item);
        }
    }

    private static PuntoVentaItemViewModel CreateOrderItem(PuntoVentaProductoViewModel product)
    {
        return new PuntoVentaItemViewModel
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
}
