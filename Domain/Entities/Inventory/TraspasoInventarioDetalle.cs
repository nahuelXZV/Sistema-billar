namespace Domain.Entities.Inventory;

public class TraspasoInventarioDetalle : Entity
{
    public long IdTraspasoInventario { get; set; }
    public long IdProducto { get; set; }
    public long? IdLote { get; set; }
    public decimal Cantidad { get; set; }

    public TraspasoInventario? TraspasoInventario { get; set; }
    public Producto? Producto { get; set; }
    public Lote? Lote { get; set; }
}
