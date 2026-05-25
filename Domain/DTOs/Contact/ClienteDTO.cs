namespace Domain.DTOs.Contact;

public class ClienteDTO
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
}
