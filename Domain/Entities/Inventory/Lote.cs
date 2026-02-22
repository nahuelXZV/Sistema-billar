
namespace Domain.Entities.Inventory;

public class Lote : Entity
{
    public string Codigo { get; set; } = string.Empty;
    public DateTime? FechaFabricacion { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public bool Activo { get; set; }
    public long IdProducto { get; set; }
}