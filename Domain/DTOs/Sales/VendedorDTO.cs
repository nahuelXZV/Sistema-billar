using Domain.DTOs.Security;

namespace Domain.DTOs.Sales;

public class VendedorDTO
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public long? IdUsuario { get; set; }
    public bool Activo { get; set; }

    public UsuarioDTO? UsuarioDTO { get; set; }
}
