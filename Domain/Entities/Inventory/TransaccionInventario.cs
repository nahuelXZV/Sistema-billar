
namespace Domain.Entities.Inventory;

public class TransaccionInventario : Entity
{
    public short Tipo { get; set; }
    public DateTime Fecha { get; set; }
    public string Glosa { get; set; } = string.Empty;
    public long IdUsuario { get; set; }
    public long IdTransaccionInicial { get; set; }

}
