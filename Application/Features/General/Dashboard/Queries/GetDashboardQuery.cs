using System.Globalization;
using Application.Interfaces;
using Domain.Common;
using Domain.DTOs.General;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.General.Dashboard.Queries;

public class GetDashboardQuery : IQuery<Response<DashboardDTO>>
{
}

public class GetDashboardQueryHandler : IQueryHandler<GetDashboardQuery, Response<DashboardDTO>>
{
    private const int MonthsToShow = 12;
    private const int WeeksToShow = 8;
    private const int RankingLimit = 7;

    private static readonly CultureInfo SpanishCulture = CultureInfo.GetCultureInfo("es-BO");

    private readonly IRepository<Venta> _ventaRepository;
    private readonly IRepository<VentaDetalle> _ventaDetalleRepository;
    private readonly IRepository<UsoMesa> _usoMesaRepository;

    public GetDashboardQueryHandler(
        IRepository<Venta> ventaRepository,
        IRepository<VentaDetalle> ventaDetalleRepository,
        IRepository<UsoMesa> usoMesaRepository)
    {
        _ventaRepository = ventaRepository;
        _ventaDetalleRepository = ventaDetalleRepository;
        _usoMesaRepository = usoMesaRepository;
    }

    public async Task<Response<DashboardDTO>> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.Today;
        var currentMonth = new DateTime(today.Year, today.Month, 1);
        var periodStart = currentMonth.AddMonths(-(MonthsToShow - 1));
        var currentWeekStart = StartOfWeek(today);
        var weeklyPeriodStart = currentWeekStart.AddDays(-7 * (WeeksToShow - 1));

        var sales = await _ventaRepository.Query()
            .Where(v => !v.Eliminado && v.Fecha >= periodStart)
            .Select(v => new SaleSummary(v.Fecha, v.Total))
            .ToListAsync(cancellationToken);

        var productSales = await _ventaDetalleRepository.Query()
            .Where(d => !d.Eliminado
                        && d.Venta != null
                        && !d.Venta.Eliminado
                        && d.Venta.Fecha >= periodStart)
            .GroupBy(d => new { d.IdProducto, d.NombreProducto })
            .Select(group => new DashboardChartItemDTO
            {
                Etiqueta = group.Key.NombreProducto,
                Valor = group.Sum(item => item.Cantidad)
            })
            .OrderByDescending(item => item.Valor)
            .Take(RankingLimit)
            .ToListAsync(cancellationToken);

        var tableUsage = await _usoMesaRepository.Query()
            .Where(u => !u.Eliminado && u.FechaInicio >= periodStart && u.Mesa != null)
            .GroupBy(u => new { u.IdMesa, u.Mesa!.Nombre })
            .Select(group => new DashboardChartItemDTO
            {
                Etiqueta = group.Key.Nombre,
                Valor = group.Count()
            })
            .OrderByDescending(item => item.Valor)
            .Take(RankingLimit)
            .ToListAsync(cancellationToken);

        var totalTableMinutes = await _usoMesaRepository.Query()
            .Where(u => !u.Eliminado && u.FechaInicio >= periodStart)
            .SumAsync(u => (double?)u.MinutosConsumidos, cancellationToken) ?? 0;

        var totalProducts = await _ventaDetalleRepository.Query()
            .Where(d => !d.Eliminado
                        && d.Venta != null
                        && !d.Venta.Eliminado
                        && d.Venta.Fecha >= periodStart)
            .SumAsync(d => (decimal?)d.Cantidad, cancellationToken) ?? 0;

        var dashboard = new DashboardDTO
        {
            FechaActualizacion = DateTime.Now,
            VentasMesActual = sales.Where(sale => sale.Date >= currentMonth).Sum(sale => sale.Total),
            VentasUltimosDoceMeses = sales.Sum(sale => sale.Total),
            CantidadVentasUltimosDoceMeses = sales.Count,
            HorasMesaUltimosDoceMeses = Math.Round(totalTableMinutes / 60, 1),
            UnidadesVendidasUltimosDoceMeses = totalProducts,
            MesasMasUsadas = tableUsage,
            ProductosMasVendidos = productSales,
            VentasPorMes = BuildMonthlySales(sales, periodStart),
            VentasPorSemana = BuildWeeklySales(sales, weeklyPeriodStart)
        };

        return new Response<DashboardDTO>(dashboard);
    }

    private static List<DashboardChartItemDTO> BuildMonthlySales(
        IReadOnlyCollection<SaleSummary> sales,
        DateTime periodStart)
    {
        return Enumerable.Range(0, MonthsToShow)
            .Select(offset =>
            {
                var month = periodStart.AddMonths(offset);
                return new DashboardChartItemDTO
                {
                    Etiqueta = SpanishCulture.TextInfo.ToTitleCase(month.ToString("MMM yy", SpanishCulture)),
                    Valor = sales
                        .Where(sale => sale.Date.Year == month.Year && sale.Date.Month == month.Month)
                        .Sum(sale => sale.Total)
                };
            })
            .ToList();
    }

    private static List<DashboardChartItemDTO> BuildWeeklySales(
        IReadOnlyCollection<SaleSummary> sales,
        DateTime periodStart)
    {
        return Enumerable.Range(0, WeeksToShow)
            .Select(offset =>
            {
                var weekStart = periodStart.AddDays(offset * 7);
                var weekEnd = weekStart.AddDays(7);
                return new DashboardChartItemDTO
                {
                    Etiqueta = $"{weekStart:dd/MM} - {weekEnd.AddDays(-1):dd/MM}",
                    Valor = sales
                        .Where(sale => sale.Date >= weekStart && sale.Date < weekEnd)
                        .Sum(sale => sale.Total)
                };
            })
            .ToList();
    }

    private static DateTime StartOfWeek(DateTime date)
    {
        var daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;
        return date.Date.AddDays(-daysSinceMonday);
    }

    private sealed record SaleSummary(DateTime Date, decimal Total);
}
