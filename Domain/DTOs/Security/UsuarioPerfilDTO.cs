namespace Domain.DTOs.Security;

public class UsuarioPerfilDTO
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool ModificarContrasena { get; set; }
    public string PasswordActual { get; set; } = string.Empty;
    public string NuevaPassword { get; set; } = string.Empty;
    public string ConfirmarPassword { get; set; } = string.Empty;
}
