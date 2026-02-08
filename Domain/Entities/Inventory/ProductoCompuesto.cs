
namespace Domain.Entities.Inventory;

public class ProductoCompuesto : Entity
{
    public long IdProductoPadre { get; set; }
    public long IdProductoComponente { get; set; }
    public double Cantidad { get; set; }
}
