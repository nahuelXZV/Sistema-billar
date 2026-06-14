using Domain.DTOs.Inventory;
using Domain.DTOs.Sales;
using WebClient.Models.Sales;

namespace WebClient.Common.Utils;

public static class PuntoVentaUtils
{
    public static PuntoVentaViewModel Create(IEnumerable<CategoriaDTO> categoriasBase, VendedorDTO? vendedor = null)
    {
        return new PuntoVentaViewModel
        {
            IdVendedor = vendedor?.Id ?? 0,
            IdListaPrecio = vendedor?.IdListaPrecio,
            NombreVendedor = vendedor?.Nombre ?? string.Empty,
            RootCategories = categoriasBase.Select(ToCategoria).ToList()
        };
    }

    public static Categorias ToCategoria(CategoriaDTO categoria)
    {
        return new Categorias
        {
            Id = categoria.Id,
            Nombre = categoria.Nombre ?? string.Empty,
            Descripcion = categoria.Descripcion ?? string.Empty,
            ImageUrl = categoria.ImagenUrl ?? string.Empty,
            IconCss = "bi bi-grid-3x3-gap",
            ToneClass = "tone-success",
        };
    }

    public static Productos ToProducto(ProductoDTO producto, string categoryLabel = "")
    {
        return new Productos
        {
            Id = producto.Id,
            Nombre = producto.Nombre,
            Descripcion = producto.Descripcion,
            ImageUrl = producto.UrlImagen ?? string.Empty,
            Precio = producto.Precio,
            IconCss = "bi bi-box-seam",
            ToneClass = "tone-primary",
        };
    }
}
