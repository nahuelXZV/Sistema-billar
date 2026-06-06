using Microsoft.AspNetCore.Components;
using WebClient.Models.Sales;
using WebClient.Services;

namespace WebClient.Components.Sales.Venta;

public partial class SeleccionarProductoComponent
{
    [CascadingParameter] public IAppServices AppServices { get; set; } = default!;
    [Parameter] public PuntoVentaViewModel PuntoVenta { get; set; } = new();
    [Parameter] public EventCallback<ProductosViewModel> OnProductoSeleccionado { get; set; }

    private void IrRaiz()
    {
        PuntoVenta.CurrentNode = null;
        PuntoVenta.SelectedPath.Clear();
    }

    private void GoToPathAsync(int index)
    {
        if (index < 0 || index >= PuntoVenta.SelectedPath.Count)
        {
            IrRaiz();
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
        return OnProductoSeleccionado.InvokeAsync(product);
    }

    private async Task LoadCategoryContentAsync(CategoriasViewModel category)
    {
        if (category.ContentLoaded)
        {
            return;
        }

        var subCategorias = await AppServices.CategoriaService.GetByCategoriaPadre(category.Id);

        category.SubCategorias = subCategorias.Select(PuntoVentaUtils.ToCategoria).ToList();

        if (category.SubCategorias.Count == 0)
        {
            var productos = PuntoVenta.IdVendedor > 0
                ? await AppServices.ProductoService.GetByCategoria(category.Id, PuntoVenta.IdVendedor)
                : [];

            category.Productos = productos.Select(producto => PuntoVentaUtils.ToProducto(producto, category.Nombre))
                .ToList();
        }

        category.ContentLoaded = true;
    }
}
