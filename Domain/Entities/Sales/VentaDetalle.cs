using Domain.Entities.Inventory;

namespace Domain.Entities.Sales;

public class VentaDetalle : Entity
{
    public long IdVenta { get; set; }
    public long? IdOrdenVentaDetalle { get; set; }
    public long IdProducto { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public double Cantidad { get; set; }
    public double PrecioUnitario { get; set; }
    public double? Descuento { get; set; }
    public double SubTotal { get; set; }
    public double Total { get; set; }

    public Venta? Venta { get; set; }
    public OrdenVentaDetalle? OrdenVentaDetalle { get; set; }
    public Producto? Producto { get; set; }
}
