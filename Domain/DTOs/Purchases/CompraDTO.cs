using Domain.DTOs.Contact;
using Domain.DTOs.Inventory;
using Domain.DTOs.Security;

namespace Domain.DTOs.Purchases;

public class CompraDTO
{
    public long Id { get; set; }
    public Guid? IdempotencyKey { get; set; }
    public string Numero { get; set; } = string.Empty;
    public long IdProveedor { get; set; }
    public long IdAlmacen { get; set; }
    public long IdUsuario { get; set; }
    public long? IdTransaccionInventario { get; set; }
    public DateTime Fecha { get; set; }
    public short Estado { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }
    public string Observacion { get; set; } = string.Empty;
    public DateTime? FechaAnulacion { get; set; }
    public long? IdUsuarioAnulacion { get; set; }
    public string? MotivoAnulacion { get; set; }

    public ProveedorDTO? Proveedor { get; set; }
    public AlmacenDTO? Almacen { get; set; }
    public UsuarioDTO? Usuario { get; set; }
    public UsuarioDTO? UsuarioAnulacion { get; set; }
    public TransaccionInventarioDTO? TransaccionInventario { get; set; }
    public List<CompraDetalleDTO> ListaDetalles { get; set; } = [];
}
