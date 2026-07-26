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

public class DashboardCajeroDTO
{
    public DateTime FechaActualizacion { get; set; }
    public string NombreVendedor { get; set; } = string.Empty;
    public bool TieneTurnoActivo { get; set; }
    public long? IdTurnoCaja { get; set; }
    public DateTime? FechaApertura { get; set; }
    public decimal TotalVendidoTurno { get; set; }
    public int CantidadVentasTurno { get; set; }
    public decimal VentasEfectivo { get; set; }
    public decimal PagosDigitales { get; set; }
    public decimal EfectivoEsperado { get; set; }
    public int MesasDisponibles { get; set; }
    public int MesasOcupadas { get; set; }
    public int MesasPorCobrar { get; set; }
    public List<DashboardMetodoPagoDTO> VentasPorMetodoPago { get; set; } = [];
}

public class DashboardMetodoPagoDTO
{
    public long IdMetodoPago { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Icono { get; set; } = string.Empty;
    public decimal MontoApertura { get; set; }
    public decimal MontoVendido { get; set; }
    public decimal MontoEsperado { get; set; }
    public bool EsEfectivo { get; set; }
}
