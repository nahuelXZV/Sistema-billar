using Domain.DTOs.Contact;
using Domain.DTOs.Inventory;
using Domain.DTOs.Sales;

namespace WebClient.Models.Sales;

public class PuntoVentaViewModel
{
    public Guid? IdempotencyKey { get; set; }
    public long? IdOrdenVenta { get; set; }
    public bool FinalizarOrdenVenta { get; set; }
    public long? IdUsoMesa { get; set; }
    public long? IdMesa { get; set; }
    public long IdVendedor { get; set; }
    public long? IdListaPrecio { get; set; }
    public string NombreVendedor { get; set; } = string.Empty;
    public long IdClienteDefault { get; set; }
    public ClienteDTO? ClienteSeleccionado { get; set; }
    public List<ClienteDTO> Clientes { get; set; } = [];

    #region Datos Venta
    public string NotaVenta { get; set; } = string.Empty;
    public decimal DescuentoGlobal { get; set; }
    public decimal RecargoGlobal { get; set; }
    public decimal Cambio => RedondearMoneda(Math.Max(0, TotalPagado - MontoTotal));
    public decimal SubTotalSeleccionado => RedondearMoneda(ProductosPagar.Where(item => item.IsSelected).Sum(item => item.Total));
    public decimal TotalPagado => RedondearMoneda(DetallePagos.Sum(payment => payment.MontoTotal));
    public decimal MontoTotal => RedondearMoneda(Math.Max(0, SubTotalSeleccionado - DescuentoGlobal + RecargoGlobal));

    public List<ItemsViewModel> DetalleItems { get; set; } = [];
    public List<DetallesPago> DetallePagos { get; set; } = [];
    public List<ProductosPagar> ProductosPagar { get; set; } = [];

    #endregion

    #region Navigation State
    public List<Categorias> RootCategories { get; set; } = [];
    public Categorias? CurrentNode { get; set; }
    public List<Categorias> SelectedPath { get; set; } = [];
    public IReadOnlyList<Categorias> VisibleCategories => CurrentNode is null ? RootCategories : CurrentNode.SubCategorias;
    public IReadOnlyList<Productos> VisibleProducts => ShowingProducts && CurrentNode is not null ? CurrentNode.Productos : [];
    public bool ShowingProducts => CurrentNode is not null && CurrentNode.SubCategorias.Count == 0;
    #endregion


    public VentaDTO GenerarDTOVenta()
    {
        this.VentaValida();
        IdempotencyKey ??= Guid.NewGuid();

        var ventadto = new VentaDTO()
        {
            IdempotencyKey = IdempotencyKey,
            Fecha = DateTime.Now,
            IdCliente = ClienteSeleccionado!.Id,
            IdOrdenVenta = IdOrdenVenta,
            FinalizarOrdenVenta = FinalizarOrdenVenta,
            IdVendedor = IdVendedor,
            TotalPagado = TotalPagado,
            Cambio = Cambio,
            Descuento = DescuentoGlobal,
            Recargo = RecargoGlobal,
            Observacion = NotaVenta,
            SubTotal = SubTotalSeleccionado,
            Total = MontoTotal,
            ListaDetalles = ProductosPagar.Where(d => d.IsSelected).Select(d => new VentaDetalleDTO()
            {
                IdOrdenVentaDetalle = d.IdOrdenVentaDetalle,
                IdProducto = d.IdProducto,
                IdProductoConversion = d.IdProductoConversion,
                Cantidad = d.CantidadPagar,
                Descuento = 0,
                PrecioUnitario = d.PrecioUnitario,
                SubTotal = d.Total,
                Total = d.Total,
                NombreProducto = d.Nombre,
                NombreUnidadMedida = d.NombreUnidadMedida,
                AbreviaturaUnidadMedida = d.AbreviaturaUnidadMedida,
                FactorConversion = d.FactorConversion
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

    private static decimal RedondearMoneda(decimal monto) =>
        Math.Round(monto, 2, MidpointRounding.AwayFromZero);
}

public class Categorias
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string IconCss { get; set; } = string.Empty;
    public string ToneClass { get; set; } = string.Empty;
    public bool ContentLoaded { get; set; }
    public List<Categorias> SubCategorias { get; set; } = [];
    public List<Productos> Productos { get; set; } = [];
}

public class Productos
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public List<ProductoPrecioVentaDTO> PreciosVenta { get; set; } = [];
    public string IconCss { get; set; } = string.Empty;
    public string ToneClass { get; set; } = string.Empty;
}

public class ProductoSeleccionado
{
    public Productos Producto { get; set; } = new();
    public ProductoPrecioVentaDTO PrecioUnidad { get; set; } = new();
}

public class ItemsViewModel
{
    public long? IdOrdenVentaDetalle { get; set; }
    public long? IdCliente { get; set; }
    public long IdProducto { get; set; }
    public long? IdProductoConversion { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string NombreUnidadMedida { get; set; } = string.Empty;
    public string AbreviaturaUnidadMedida { get; set; } = string.Empty;
    public decimal FactorConversion { get; set; } = 1;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public bool EsTiempoMesa { get; set; }
    public decimal Total => Math.Round(Cantidad * PrecioUnitario, 2, MidpointRounding.AwayFromZero);
}

public class CantidadModificada
{
    public long ProductId { get; set; }
    public long? ProductConversionId { get; set; }
    public long? IdCliente { get; set; }
    public bool EsTiempoMesa { get; set; }
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
    public long? IdOrdenVentaDetalle { get; set; }
    public long? IdCliente { get; set; }
    public long IdProducto { get; set; }
    public long? IdProductoConversion { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string NombreUnidadMedida { get; set; } = string.Empty;
    public string AbreviaturaUnidadMedida { get; set; } = string.Empty;
    public decimal FactorConversion { get; set; } = 1;
    public decimal CantidadDisponible { get; set; }
    public decimal CantidadPagar { get; set; }
    public decimal PrecioUnitario { get; set; }
    public bool EsTiempoMesa { get; set; }
    public bool IsSelected { get; set; }
    public decimal Total => Math.Round(CantidadPagar * PrecioUnitario, 2, MidpointRounding.AwayFromZero);
}
