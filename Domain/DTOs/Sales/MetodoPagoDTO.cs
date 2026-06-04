namespace Domain.DTOs.Sales;

public class MetodoPagoDTO
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Abreviatura { get; set; } = string.Empty;
    public string ClaveMoneda { get; set; } = string.Empty;
    public string Icono { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public bool Eliminado { get; set; }
}
