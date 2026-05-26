using Domain.DTOs.Configuration;

namespace Domain.DTOs.Sales;

public class UsoMesaDTO
{
    public long Id { get; set; }
    public long IdOrdenVenta { get; set; }
    public long IdMesa { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public double MinutosConsumidos { get; set; }
    public double TarifaAplicada { get; set; }
    public double MontoCalculado { get; set; }
    public short Estado { get; set; }
    public string Observacion { get; set; } = string.Empty;

    public MesaDTO? Mesa { get; set; }
}
