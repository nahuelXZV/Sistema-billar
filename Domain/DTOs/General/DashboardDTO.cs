namespace Domain.DTOs.General;

public class DashboardDTO
{
    public DateTime FechaActualizacion { get; set; }
    public decimal VentasMesActual { get; set; }
    public decimal VentasUltimosDoceMeses { get; set; }
    public int CantidadVentasUltimosDoceMeses { get; set; }
    public double HorasMesaUltimosDoceMeses { get; set; }
    public decimal UnidadesVendidasUltimosDoceMeses { get; set; }
    public List<DashboardChartItemDTO> MesasMasUsadas { get; set; } = [];
    public List<DashboardChartItemDTO> ProductosMasVendidos { get; set; } = [];
    public List<DashboardChartItemDTO> VentasPorMes { get; set; } = [];
    public List<DashboardChartItemDTO> VentasPorSemana { get; set; } = [];
}

public class DashboardChartItemDTO
{
    public string Etiqueta { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}
