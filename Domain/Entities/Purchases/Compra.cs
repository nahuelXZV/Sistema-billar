using Domain.Entities.Contact;
using Domain.Entities.Inventory;
using Domain.Entities.Security;

namespace Domain.Entities.Purchases;

public class Compra : Entity
{
    public Guid? IdempotencyKey { get; set; }
    public string Numero { get; set; } = string.Empty;
    public long IdProveedor { get; set; }
    public long IdAlmacen { get; set; }
    public long IdUsuario { get; set; }
    public long? IdTransaccionInventario { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public short Estado { get; set; } = 1;
    public decimal SubTotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }
    public string Observacion { get; set; } = string.Empty;
    public DateTime? FechaAnulacion { get; set; }
    public long? IdUsuarioAnulacion { get; set; }
    public string? MotivoAnulacion { get; set; }

    public Proveedor? Proveedor { get; set; }
    public Almacen? Almacen { get; set; }
    public Usuario? Usuario { get; set; }
    public Usuario? UsuarioAnulacion { get; set; }
    public TransaccionInventario? TransaccionInventario { get; set; }
    public List<CompraDetalle> ListaDetalles { get; set; } = [];
}
