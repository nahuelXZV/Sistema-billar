
namespace Domain.Entities.Inventory;

public class Categoria : Entity
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int OrdenVisual { get; set; }
    public string ImagenUrl { get; set; } = string.Empty;
    public long? IdCategoriaPadre { get; set; }
    public bool Activo { get; set; }
}
