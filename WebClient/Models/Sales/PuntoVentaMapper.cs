using Domain.DTOs.Inventory;
using Domain.DTOs.Sales;

namespace WebClient.Models.Sales;

public static class PuntoVentaMapper
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

    public static CategoriasViewModel ToCategoria(CategoriaDTO categoria)
    {
        return new CategoriasViewModel
        {
            Id = categoria.Id.ToString(),
            CategoriaId = categoria.Id,
            Name = categoria.Nombre ?? string.Empty,
            Description = categoria.Descripcion ?? string.Empty,
            ImageUrl = categoria.ImagenUrl ?? string.Empty,
            IconCss = "bi bi-grid-3x3-gap",
            ToneClass = "tone-success",
            CardCaption = categoria.Nombre ?? string.Empty
        };
    }

    public static ProductosViewModel ToProducto(ProductoDTO producto, string categoryLabel = "")
    {
        return new ProductosViewModel
        {
            Id = producto.Id.ToString(),
            ProductoId = producto.Id,
            Name = producto.Nombre,
            Description = producto.Descripcion,
            ImageUrl = producto.UrlImagen ?? string.Empty,
            Price = producto.Precio,
            CategoryLabel = categoryLabel,
            IconCss = "bi bi-box-seam",
            ToneClass = "tone-primary",
            MediaLabel = producto.Nombre
        };
    }
}
