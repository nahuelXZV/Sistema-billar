using Microsoft.AspNetCore.Components;
using WebClient.Models.Sales;
using WebClient.Services;

namespace WebClient.Components.Sales.Venta;

public partial class SeleccionarProductoComponent
{
    [CascadingParameter] public IAppServices AppServices { get; set; } = default!;
    [Parameter] public PuntoVentaViewModel PuntoVenta { get; set; } = new();
    [Parameter] public EventCallback<ProductosViewModel> OnProductSelected { get; set; }

    private void GoToRootAsync()
    {
        PuntoVenta.CurrentNode = null;
        PuntoVenta.SelectedPath.Clear();
    }

    private void GoToPathAsync(int index)
    {
        if (index < 0 || index >= PuntoVenta.SelectedPath.Count)
        {
            GoToRootAsync();
            return;
        }

        PuntoVenta.CurrentNode = PuntoVenta.SelectedPath[index];
        PuntoVenta.SelectedPath.RemoveRange(index + 1, PuntoVenta.SelectedPath.Count - (index + 1));
    }

    private async Task EnterCategoryAsync(CategoriasViewModel category)
    {
        await LoadCategoryContentAsync(category);

        PuntoVenta.CurrentNode = category;

        if (PuntoVenta.SelectedPath.Count == 0)
        {
            PuntoVenta.SelectedPath.Add(category);
            return;
        }

        var existingIndex = PuntoVenta.SelectedPath.FindIndex(pathNode => pathNode.Id == category.Id);
        if (existingIndex >= 0)
        {
            GoToPathAsync(existingIndex);
            return;
        }

        PuntoVenta.SelectedPath.Add(category);
    }

    private Task SelectProductAsync(ProductosViewModel product)
    {
        return OnProductSelected.InvokeAsync(product);
    }

    private async Task LoadCategoryContentAsync(CategoriasViewModel category)
    {
        if (category.ContentLoaded)
        {
            return;
        }

        var subCategorias = await AppServices.CategoriaService.GetByCategoriaPadre(category.CategoriaId);

        category.Children = subCategorias
            .Select(PuntoVentaMapper.ToCategoria)
            .ToList();

        if (category.Children.Count == 0)
        {
            var productos = PuntoVenta.IdVendedor > 0
                ? await AppServices.ProductoService.GetByCategoria(category.CategoriaId, PuntoVenta.IdVendedor)
                : [];

            category.Products = productos
                .Select(producto => PuntoVentaMapper.ToProducto(producto, category.Name))
                .ToList();
        }

        category.ContentLoaded = true;
    }

    private static string FormatMoney(decimal amount)
    {
        return $"Bs {amount:N2}";
    }
}
