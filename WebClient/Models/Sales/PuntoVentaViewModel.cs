namespace WebClient.Models.Sales;

public class PuntoVentaViewModel
{
    public long IdVendedor { get; set; }
    public long? IdListaPrecio { get; set; }
    public string NombreVendedor { get; set; } = string.Empty;
    public string CustomerSearchPlaceholder { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public decimal ServiceCharge { get; set; }
    public List<PuntoVentaMetodoPagoViewModel> PaymentMethods { get; set; } = [];
    public List<PuntoVentaCategoriaViewModel> RootCategories { get; set; } = [];
    public List<PuntoVentaItemViewModel> OrderItems { get; set; } = [];
    public PuntoVentaCategoriaViewModel? CurrentNode { get; set; }
    public List<PuntoVentaCategoriaViewModel> SelectedPath { get; set; } = [];

    public IReadOnlyList<PuntoVentaCategoriaViewModel> VisibleCategories =>
        CurrentNode is null ? RootCategories : CurrentNode.Children;

    public IReadOnlyList<PuntoVentaProductoViewModel> VisibleProducts =>
        ShowingProducts && CurrentNode is not null ? CurrentNode.Products : [];

    public bool ShowingProducts =>
        CurrentNode is not null && CurrentNode.Children.Count == 0;

    public decimal Subtotal => OrderItems.Sum(item => item.Total);
    public decimal GrandTotal => Subtotal - DiscountAmount + ServiceCharge;
}

public class PuntoVentaCategoriaViewModel
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
    public List<PuntoVentaCategoriaViewModel> Children { get; set; } = [];
    public List<PuntoVentaProductoViewModel> Products { get; set; } = [];
}

public class PuntoVentaProductoViewModel
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
    public List<string> Tags { get; set; } = [];
}

public class PuntoVentaItemViewModel
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

public class PuntoVentaMetodoPagoViewModel
{
    public string Name { get; set; } = string.Empty;
    public string IconCss { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}
