namespace Domain.Entities.Inventory;

public class ProductoConversion : Entity
{
    public long IdProducto { get; set; }
    public long IdUnidadMedida { get; set; }
    public decimal FactorConversion { get; set; }

    public Producto? Producto { get; set; }
    public UnidadMedida? UnidadMedida { get; set; }
}
