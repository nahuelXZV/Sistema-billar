using Domain.Entities.Security;
using Domain.Entities.Inventory;

namespace Domain.Entities.Sales;

public class Vendedor : Entity
{
    public string Nombre { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public long? IdUsuario { get; set; }
    public long? IdListaPrecio { get; set; }
    public bool Activo { get; set; }

    public Usuario? Usuario { get; set; }
    public ListaPrecios? ListaPrecio { get; set; }
    public List<VendedorAlmacenes> ListaAlmacenes { get; set; } = new();
}
