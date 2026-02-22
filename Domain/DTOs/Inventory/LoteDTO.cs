
namespace Domain.DTOs.Inventory;

public class LoteDTO
{
    public long Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public DateTime? FechaFabricacion { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public bool Activo { get; set; }
    public long IdProducto { get; set; }
}
