namespace Domain.DTOs.Sales;

public class PagoVentaDTO
{
    public long Id { get; set; }
    public long IdVenta { get; set; }
    public long IdMetodoPago { get; set; }
    public DateTime Fecha { get; set; }
    public decimal MontoTotal { get; set; }
    public string Observacion { get; set; } = string.Empty;

    public VentaDTO? Venta { get; set; }
    public MetodoPagoDTO? MetodoPago { get; set; }
}
