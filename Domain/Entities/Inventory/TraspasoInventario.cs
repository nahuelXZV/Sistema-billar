using Domain.Entities.Security;

namespace Domain.Entities.Inventory;

public class TraspasoInventario : Entity
{
    public long IdAlmacenOrigen { get; set; }
    public long IdAlmacenDestino { get; set; }
    public long IdUsuario { get; set; }
    public DateTime Fecha { get; set; }
    public string Glosa { get; set; } = string.Empty;
    public short Estado { get; set; }

    public Almacen? AlmacenOrigen { get; set; }
    public Almacen? AlmacenDestino { get; set; }
    public Usuario? Usuario { get; set; }
    public List<TraspasoInventarioDetalle> Detalles { get; set; } = [];
}
