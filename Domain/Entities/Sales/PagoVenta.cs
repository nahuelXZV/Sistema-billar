namespace Domain.Entities.Sales;

public class PagoVenta : Entity
{
    public long IdVenta { get; set; }
    public long IdMetodoPago { get; set; }
    public DateTime Fecha { get; set; }
    public decimal MontoTotal { get; set; }
    public string Observacion { get; set; } = string.Empty;

    public Venta? Venta { get; set; }
    public MetodoPago? MetodoPago { get; set; }
}
