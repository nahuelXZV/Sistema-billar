
namespace Domain.DTOs.Inventory;

public class CategoriaDTO
{
    public long Id { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public int OrdenVisual { get; set; }
    public string ImagenUrl { get; set; }
    public long? IdCategoriaPadre { get; set; }
    public bool Activo { get; set; }

    public List<CategoriaDTO> SubCategorias { get; set; } = new();
}
