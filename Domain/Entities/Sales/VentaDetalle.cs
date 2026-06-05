using Domain.Entities.Inventory;

namespace Domain.Entities.Sales;

public class VentaDetalle : Entity
{
    public long IdVenta { get; set; }
    public long? IdOrdenVentaDetalle { get; set; }
    public long IdProducto { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal? Descuento { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Total { get; set; }

    public Venta? Venta { get; set; }
    public OrdenVentaDetalle? OrdenVentaDetalle { get; set; }
    public Producto? Producto { get; set; }
}
