
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Inventory;

public class Producto : Entity
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public string UrlImagen { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public bool EsCompuesto { get; set; }
    public long IdCategoria { get; set; }
    public long IdUnidadMedida { get; set; }

    [NotMapped]
    public Categoria? Categoria { get; set; }
    [NotMapped]
    public UnidadMedida? UnidadMedida { get; set; }
}
