namespace Domain.DTOs.Sales;

public class OrdenMesaDTO
{
    public long IdOrdenVenta { get; set; }
    public long IdUsoMesa { get; set; }
    public long IdMesa { get; set; }
    public long? IdCliente { get; set; }
    public long IdVendedor { get; set; }
    public string Numero { get; set; } = string.Empty;
    public short EstadoOrden { get; set; }
    public short EstadoUsoMesa { get; set; }
    public DateTime FechaApertura { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public double MinutosConsumidos { get; set; }
    public decimal TarifaAplicada { get; set; }
    public decimal MontoCalculado { get; set; }
    public decimal DescuentoGlobal { get; set; }
    public decimal RecargoGlobal { get; set; }
    public decimal Total { get; set; }
    public string Observacion { get; set; } = string.Empty;
    public List<OrdenMesaDetalleDTO> Detalles { get; set; } = [];
}
