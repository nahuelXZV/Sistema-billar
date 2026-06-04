namespace Domain.Entities.Sales;

public class MetodoPago : Entity
{
    public string Nombre { get; set; } = string.Empty;
    public string Abreviatura { get; set; } = string.Empty;
    public string ClaveMoneda { get; set; } = string.Empty;
    public string Icono { get; set; } = string.Empty;
    public bool Activo { get; set; }
}
