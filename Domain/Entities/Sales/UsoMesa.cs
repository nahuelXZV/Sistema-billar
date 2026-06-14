using Domain.Entities.Configuration;

namespace Domain.Entities.Sales;

public class UsoMesa : Entity
{
    public long IdOrdenVenta { get; set; }
    public long IdMesa { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public decimal MinutosConsumidos { get; set; }
    public decimal TarifaAplicada { get; set; }
    public decimal MontoCalculado { get; set; }
    public short Estado { get; set; }
    public string Observacion { get; set; } = string.Empty;

    public OrdenVenta? OrdenVenta { get; set; }
    public Mesa? Mesa { get; set; }
}
