
namespace Domain.Entities.Inventory;

public class Categoria : Entity
{
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public int OrdenVisual { get; set; }
    public string ImagenUrl { get; set; }
    public long? IdCategoriaPadre { get; set; }
    public bool Activo { get; set; }
}
