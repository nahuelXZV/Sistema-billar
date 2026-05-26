using Domain.DTOs.Inventory;

namespace Domain.DTOs.Sales;

public class VentaDetalleDTO
{
    public long Id { get; set; }
    public long IdVenta { get; set; }
    public long? IdOrdenVentaDetalle { get; set; }
    public long IdProducto { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public double Cantidad { get; set; }
    public double PrecioUnitario { get; set; }
    public double? Descuento { get; set; }
    public double SubTotal { get; set; }
    public double Total { get; set; }

    public ProductoDTO? Producto { get; set; }
    public OrdenVentaDetalleDTO? OrdenVentaDetalle { get; set; }
}
