
namespace Domain.Entities.Inventory;

public class ListaPreciosDetalle : Entity
{
    public long IdListaPrecio { get; set; }
    public long IdProductoConversion { get; set; }
    public decimal Precio { get; set; }

    public ProductoConversion? ProductoConversion { get; set; }
}
