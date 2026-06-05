using Domain.Entities.Inventory;

namespace Domain.Entities.Sales;

public class OrdenVentaDetalle : Entity
{
    public long IdOrdenVenta { get; set; }
    public long IdProducto { get; set; }
    public long? IdUsoMesa { get; set; }
    public long IdVendedor { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Descuento { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Total { get; set; }
    public short Estado { get; set; }
    public string Observacion { get; set; } = string.Empty;

    public OrdenVenta? OrdenVenta { get; set; }
    public Producto? Producto { get; set; }
    public UsoMesa? UsoMesa { get; set; }
    public Vendedor? Vendedor { get; set; }
}
