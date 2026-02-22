
namespace Domain.Entities.Inventory;

public class ListaPrecios : Entity
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; }

    public List<ListaPreciosDetalle>? ListaDetalles { get; set; }
}