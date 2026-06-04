using Domain.DTOs.Contact;

namespace WebClient.Models.Sales;

public class PuntoVentaViewModel
{
    public long IdVendedor { get; set; }
    public long? IdListaPrecio { get; set; }
    public string NombreVendedor { get; set; } = string.Empty;
    public ClienteDTO? ClienteSeleccionado { get; set; }


    public string NotaVenta { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public decimal ServiceCharge { get; set; }

    public List<ItemsViewModel> OrderItems { get; set; } = [];
    public decimal Subtotal => OrderItems.Sum(item => item.Total);
    public decimal GrandTotal => Subtotal - DiscountAmount + ServiceCharge;

    #region Navigation State
    public List<CategoriasViewModel> RootCategories { get; set; } = [];
    public CategoriasViewModel? CurrentNode { get; set; }
    public List<CategoriasViewModel> SelectedPath { get; set; } = [];
    public IReadOnlyList<CategoriasViewModel> VisibleCategories => CurrentNode is null ? RootCategories : CurrentNode.Children;
    public IReadOnlyList<ProductosViewModel> VisibleProducts => ShowingProducts && CurrentNode is not null ? CurrentNode.Products : [];
    public bool ShowingProducts => CurrentNode is not null && CurrentNode.Children.Count == 0;
    #endregion
}

public class CategoriasViewModel
{
    public string Id { get; set; } = string.Empty;
    public long CategoriaId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string IconCss { get; set; } = string.Empty;
    public string ToneClass { get; set; } = string.Empty;
    public string CardCaption { get; set; } = string.Empty;
    public bool ContentLoaded { get; set; }
    public List<CategoriasViewModel> Children { get; set; } = [];
    public List<ProductosViewModel> Products { get; set; } = [];
}

public class ProductosViewModel
{
    public string Id { get; set; } = string.Empty;
    public long ProductoId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string CategoryLabel { get; set; } = string.Empty;
    public string IconCss { get; set; } = string.Empty;
    public string ToneClass { get; set; } = string.Empty;
    public string MediaLabel { get; set; } = string.Empty;
}

public class ItemsViewModel
{
    public string ProductId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string IconCss { get; set; } = string.Empty;
    public string ToneClass { get; set; } = string.Empty;
    public decimal Total => Quantity * UnitPrice;
}

public class PagoItemViewModel
{
    public string ProductId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total => Quantity * UnitPrice;
}
