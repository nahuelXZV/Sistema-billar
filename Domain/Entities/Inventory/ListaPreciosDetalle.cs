
namespace Domain.Entities.Inventory;

public class ListaPreciosDetalle : Entity
{
    public long IdListaPrecio { get; set; }
    public long IdProducto { get; set; }
    public double Precio { get; set; }

    public Producto? Producto { get; set; }
}