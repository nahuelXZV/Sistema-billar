namespace Domain.Entities.Sales;

public class TurnoCaja : Entity
{
    public long IdVendedor { get; set; }
    public DateTime FechaApertura { get; set; }
    public DateTime? FechaCierre { get; set; }
    public short Estado { get; set; }
    public string Observacion { get; set; } = string.Empty;

    public Vendedor? Vendedor { get; set; }
    public List<TurnoCajaDetalle> Detalles { get; set; } = [];
}
