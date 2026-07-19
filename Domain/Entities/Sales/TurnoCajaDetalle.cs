namespace Domain.Entities.Sales;

public class TurnoCajaDetalle : Entity
{
    public long IdTurnoCaja { get; set; }
    public long IdMetodoPago { get; set; }
    public decimal MontoApertura { get; set; }
    public decimal? MontoVentasSistema { get; set; }
    public decimal? MontoCierreDeclarado { get; set; }
    public decimal? Diferencia { get; set; }

    public TurnoCaja? TurnoCaja { get; set; }
    public MetodoPago? MetodoPago { get; set; }
}
