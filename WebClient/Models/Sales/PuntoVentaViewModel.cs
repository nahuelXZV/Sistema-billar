using Domain.DTOs.Contact;

namespace WebClient.Models.Sales;

public class PuntoVentaViewModel
{
    public long IdVendedor { get; set; }
    public long? IdListaPrecio { get; set; }
    public string NombreVendedor { get; set; } = string.Empty;
    public ClienteDTO? ClienteSeleccionado { get; set; }

    #region Datos Venta
    public string NotaVenta { get; set; } = string.Empty;
    public decimal DescuentoGlobal { get; set; }
    public decimal RecargoTotal { get; set; }
    public List<ItemsViewModel> DetalleItems { get; set; } = [];
    #endregion

    public decimal Subtotal => DetalleItems.Sum(item => item.Total);
    public decimal Total => Subtotal - DescuentoGlobal + RecargoTotal;

    #region Navigation State
    public List<CategoriasViewModel> RootCategories { get; set; } = [];
    public CategoriasViewModel? CurrentNode { get; set; }
    public List<CategoriasViewModel> SelectedPath { get; set; } = [];
    public IReadOnlyList<CategoriasViewModel> VisibleCategories => CurrentNode is null ? RootCategories : CurrentNode.SubCategorias;
    public IReadOnlyList<ProductosViewModel> VisibleProducts => ShowingProducts && CurrentNode is not null ? CurrentNode.Productos : [];
    public bool ShowingProducts => CurrentNode is not null && CurrentNode.SubCategorias.Count == 0;
    #endregion
}

public class CategoriasViewModel
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string IconCss { get; set; } = string.Empty;
    public string ToneClass { get; set; } = string.Empty;
    public bool ContentLoaded { get; set; }
    public List<CategoriasViewModel> SubCategorias { get; set; } = [];
    public List<ProductosViewModel> Productos { get; set; } = [];
}

public class ProductosViewModel
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string IconCss { get; set; } = string.Empty;
    public string ToneClass { get; set; } = string.Empty;
}

public class ItemsViewModel
{
    public long ProductId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Total => Cantidad * PrecioUnitario;
}

public class CantidadModificada
{
    public long ProductId { get; set; }
    public decimal Cantidad { get; set; }
}

public class DetallePagoViewModel
{
    public string Id { get; set; } = string.Empty;
    public long IdMetodoPago { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Abreviatura { get; set; } = string.Empty;
    public string Icono { get; set; } = string.Empty;
    public decimal Monto { get; set; }
}

public class ModalPagoItemViewModel
{
    public long ProductId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal CantidadDisponible { get; set; }
    public decimal CantidadPagar { get; set; }
    public decimal PrecioUnitario { get; set; }
    public bool IsSelected { get; set; }
    public decimal Total => CantidadPagar * PrecioUnitario;
}