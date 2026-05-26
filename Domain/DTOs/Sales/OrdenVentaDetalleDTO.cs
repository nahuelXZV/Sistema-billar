using Domain.DTOs.Inventory;

namespace Domain.DTOs.Sales;

public class OrdenVentaDetalleDTO
{
    public long Id { get; set; }
    public long IdOrdenVenta { get; set; }
    public long IdProducto { get; set; }
    public long? IdUsoMesa { get; set; }
    public long IdVendedor { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public double Cantidad { get; set; }
    public double PrecioUnitario { get; set; }
    public double Descuento { get; set; }
    public double SubTotal { get; set; }
    public double Total { get; set; }
    public short Estado { get; set; }
    public string Observacion { get; set; } = string.Empty;

    public ProductoDTO? Producto { get; set; }
    public UsoMesaDTO? UsoMesa { get; set; }
    public VendedorDTO? Vendedor { get; set; }
}
