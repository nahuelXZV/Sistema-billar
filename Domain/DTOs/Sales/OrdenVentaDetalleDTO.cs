using Domain.DTOs.Contact;
using Domain.DTOs.Inventory;

namespace Domain.DTOs.Sales;

public class OrdenVentaDetalleDTO
{
    public long Id { get; set; }
    public long IdOrdenVenta { get; set; }
    public long? IdCliente { get; set; }
    public long IdProducto { get; set; }
    public long? IdProductoConversion { get; set; }
    public long? IdUsoMesa { get; set; }
    public long IdVendedor { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string NombreUnidadMedida { get; set; } = string.Empty;
    public string AbreviaturaUnidadMedida { get; set; } = string.Empty;
    public decimal FactorConversion { get; set; } = 1;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Descuento { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Total { get; set; }
    public short Estado { get; set; }
    public string Observacion { get; set; } = string.Empty;

    public ProductoDTO? Producto { get; set; }
    public ClienteDTO? Cliente { get; set; }
    public ProductoConversionDTO? ProductoConversion { get; set; }
    public UsoMesaDTO? UsoMesa { get; set; }
    public VendedorDTO? Vendedor { get; set; }
}
