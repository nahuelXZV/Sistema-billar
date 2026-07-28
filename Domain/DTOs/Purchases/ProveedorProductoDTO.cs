using Domain.DTOs.Inventory;

namespace Domain.DTOs.Purchases;

public class ProveedorProductoDTO
{
    public long Id { get; set; }
    public long IdProveedor { get; set; }
    public long IdProducto { get; set; }
    public long? IdProductoConversion { get; set; }
    public decimal CostoReferencial { get; set; }
    public DateTime FechaActualizacion { get; set; }

    public ProductoDTO? Producto { get; set; }
    public ProductoConversionDTO? ProductoConversion { get; set; }
}
