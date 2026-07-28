using Domain.Entities.Contact;
using Domain.Entities.Inventory;

namespace Domain.Entities.Purchases;

public class ProveedorProducto : Entity
{
    public long IdProveedor { get; set; }
    public long IdProducto { get; set; }
    public long? IdProductoConversion { get; set; }
    public decimal CostoReferencial { get; set; }
    public DateTime FechaActualizacion { get; set; } = DateTime.Now;

    public Proveedor? Proveedor { get; set; }
    public Producto? Producto { get; set; }
    public ProductoConversion? ProductoConversion { get; set; }
}
