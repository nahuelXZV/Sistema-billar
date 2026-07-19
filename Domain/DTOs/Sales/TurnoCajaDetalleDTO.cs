namespace Domain.DTOs.Sales;

public class TurnoCajaDetalleDTO
{
    public long Id { get; set; }
    public long IdTurnoCaja { get; set; }
    public long IdMetodoPago { get; set; }
    public decimal MontoApertura { get; set; }
    public decimal? MontoVentasSistema { get; set; }
    public decimal? MontoCierreDeclarado { get; set; }
    public decimal? Diferencia { get; set; }

    public MetodoPagoDTO? MetodoPago { get; set; }
}
