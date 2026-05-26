namespace Domain.Entities.Sales;

public class PagoVenta : Entity
{
    public long IdVenta { get; set; }
    public short CodigoMoneda { get; set; }
    public DateTime Fecha { get; set; }
    public double MontoTotal { get; set; }
    public double MontoRecibido { get; set; }
    public double MontoCambio { get; set; }
    public string Observacion { get; set; } = string.Empty;

    public Venta? Venta { get; set; }
}
