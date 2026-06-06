using Domain.DTOs.Contact;
using Domain.DTOs.Sales;

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
    public decimal RecargoGlobal { get; set; }
    public decimal Cambio => Math.Max(0, TotalPagado - MontoTotal);
    public decimal SubTotalSeleccionado => ProductosPagar.Where(item => item.IsSelected).Sum(item => item.Total);
    public decimal TotalPagado => DetallePagos.Sum(payment => payment.MontoTotal);
    public decimal MontoTotal => Math.Max(0, SubTotalSeleccionado - DescuentoGlobal + RecargoGlobal);

    public List<ItemsViewModel> DetalleItems { get; set; } = [];
    public List<DetallesPago> DetallePagos { get; set; } = [];
    public List<ProductosPagar> ProductosPagar { get; set; } = [];

    #endregion

    #region Navigation State
    public List<CategoriasViewModel> RootCategories { get; set; } = [];
    public CategoriasViewModel? CurrentNode { get; set; }
    public List<CategoriasViewModel> SelectedPath { get; set; } = [];
    public IReadOnlyList<CategoriasViewModel> VisibleCategories => CurrentNode is null ? RootCategories : CurrentNode.SubCategorias;
    public IReadOnlyList<ProductosViewModel> VisibleProducts => ShowingProducts && CurrentNode is not null ? CurrentNode.Productos : [];
    public bool ShowingProducts => CurrentNode is not null && CurrentNode.SubCategorias.Count == 0;
    #endregion


    public VentaDTO GenerarDTOVenta()
    {
        this.VentaValida();

        var ventadto = new VentaDTO()
        {
            Numero = DateTime.Now.ToString("HHmmss"),
            Fecha = DateTime.Now,
            IdCliente = ClienteSeleccionado!.Id,
            IdOrdenVenta = 0,
            IdVendedor = IdVendedor,
            TotalPagado = TotalPagado,
            Cambio = Cambio,
            Descuento = DescuentoGlobal,
            Observacion = NotaVenta,
            SubTotal = SubTotalSeleccionado,
            Total = MontoTotal,
            ListaDetalles = ProductosPagar.Where(d => d.IsSelected).Select(d => new VentaDetalleDTO()
            {
                IdOrdenVentaDetalle = 0,
                IdProducto = d.IdProducto,
                Cantidad = d.CantidadPagar,
                Descuento = 0,
                PrecioUnitario = d.PrecioUnitario,
                SubTotal = d.Total,
                Total = d.Total,
                NombreProducto = d.Nombre
            }).ToList(),
            ListaPagos = DetallePagos.Select(p => new PagoVentaDTO()
            {
                IdMetodoPago = p.IdMetodoPago,
                Fecha = DateTime.Now,
                MontoTotal = p.MontoTotal,
                Observacion = $"Pago de {p.MontoTotal} con {p.Nombre}",
            }).ToList()
        };

        return ventadto;
    }

    private void VentaValida()
    {
        if (ClienteSeleccionado == null)
        {
            throw new Exception("Debe seleccionar un cliente");
        }

    }
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
    public long IdProducto { get; set; }
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

public class DetallesPago
{
    public string Id { get; set; } = string.Empty;
    public long IdMetodoPago { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Abreviatura { get; set; } = string.Empty;
    public string Icono { get; set; } = string.Empty;

    public decimal MontoTotal { get; set; }
}

public class ProductosPagar
{
    public long IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal CantidadDisponible { get; set; }
    public decimal CantidadPagar { get; set; }
    public decimal PrecioUnitario { get; set; }
    public bool IsSelected { get; set; }
    public decimal Total => CantidadPagar * PrecioUnitario;
}