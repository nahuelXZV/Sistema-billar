using System.Globalization;
using Application.Interfaces;
using Domain.Common;
using Domain.Constants;
using Domain.DTOs.General;
using Domain.Entities.Inventory;
using Domain.Entities.Purchases;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.General.Dashboard.Queries;

public class GetDashboardQuery : IQuery<Response<DashboardDTO>>
{
    public int? Mes { get; set; }
    public int? Anio { get; set; }
}

public class GetDashboardQueryHandler : IQueryHandler<GetDashboardQuery, Response<DashboardDTO>>
{
    private const int MonthsToShow = 12;
    private const int RankingLimit = 5;
    private const string SaleInventoryMovementDescription = "Salida por venta";

    private static readonly CultureInfo SpanishCulture = CultureInfo.GetCultureInfo("es-BO");

    private readonly IRepository<Venta> _ventaRepository;
    private readonly IRepository<Compra> _compraRepository;
    private readonly IRepository<VentaDetalle> _ventaDetalleRepository;
    private readonly IRepository<CompraDetalle> _compraDetalleRepository;
    private readonly IRepository<UsoMesa> _usoMesaRepository;

    public GetDashboardQueryHandler(
        IRepository<Venta> ventaRepository,
        IRepository<Compra> compraRepository,
        IRepository<VentaDetalle> ventaDetalleRepository,
        IRepository<CompraDetalle> compraDetalleRepository,
        IRepository<UsoMesa> usoMesaRepository)
    {
        _ventaRepository = ventaRepository;
        _compraRepository = compraRepository;
        _ventaDetalleRepository = ventaDetalleRepository;
        _compraDetalleRepository = compraDetalleRepository;
        _usoMesaRepository = usoMesaRepository;
    }

    public async Task<Response<DashboardDTO>> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.Today;
        var selectedYear = request.Anio is >= 2000 and <= 2100 ? request.Anio.Value : today.Year;
        var selectedMonthNumber = request.Mes is >= 1 and <= 12 ? request.Mes.Value : today.Month;
        var selectedMonth = new DateTime(selectedYear, selectedMonthNumber, 1);
        var selectedMonthEnd = selectedMonth.AddMonths(1);
        var periodStart = selectedMonth.AddMonths(-(MonthsToShow - 1));

        var sales = await _ventaRepository.Query()
            .Where(v => !v.Eliminado && v.Fecha >= periodStart && v.Fecha < selectedMonthEnd)
            .Select(v => new SaleSummary(v.Fecha, v.Total))
            .ToListAsync(cancellationToken);

        var ventasNetas = sales
            .Where(sale => sale.Date >= selectedMonth && sale.Date < selectedMonthEnd)
            .Sum(sale => sale.Total);

        var purchases = await _compraRepository.Query()
            .Where(compra => !compra.Eliminado
                             && compra.Estado != (short)Constantes.EstadoCompra.Anulada
                             && compra.Fecha >= periodStart
                             && compra.Fecha < selectedMonthEnd)
            .Select(compra => new PurchaseSummary(
                compra.Fecha,
                compra.Total,
                compra.Proveedor != null && !string.IsNullOrWhiteSpace(compra.Proveedor.NombreComercial)
                    ? compra.Proveedor.NombreComercial
                    : "Sin proveedor"))
            .ToListAsync(cancellationToken);

        var compras = purchases
            .Where(purchase => purchase.Date >= selectedMonth && purchase.Date < selectedMonthEnd)
            .Sum(purchase => purchase.Total);

        var productSales = await _ventaDetalleRepository.Query()
            .Where(detail => !detail.Eliminado
                             && detail.Venta != null
                             && !detail.Venta.Eliminado
                             && detail.Venta.Fecha >= selectedMonth
                             && detail.Venta.Fecha < selectedMonthEnd)
            .GroupBy(detail => new { detail.IdProducto, detail.NombreProducto })
            .Select(group => new ProductSaleSummary(
                group.Key.IdProducto,
                group.Key.NombreProducto,
                group.Sum(detail => detail.Cantidad * detail.FactorConversion),
                group.Sum(detail => detail.Total)))
            .ToListAsync(cancellationToken);

        var productCosts = await _compraDetalleRepository.Query()
            .Where(detail => !detail.Eliminado
                             && detail.Compra != null
                             && !detail.Compra.Eliminado
                             && detail.Compra.Estado != (short)Constantes.EstadoCompra.Anulada
                             && detail.Compra.Fecha < selectedMonthEnd)
            .GroupBy(detail => detail.IdProducto)
            .Select(group => new ProductCostSummary(
                group.Key,
                group.Sum(detail => detail.CantidadBase),
                group.Sum(detail => detail.Total)))
            .ToListAsync(cancellationToken);

        var inventoryOutputs = await (
                from detail in _ventaRepository.Query<TransaccionInventarioDetalle>()
                join movement in _ventaRepository.Query<TransaccionInventario>()
                    on detail.IdTransaccion equals movement.Id
                join sale in _ventaRepository.Query()
                    on movement.IdTransaccionInicial equals sale.Id
                where !detail.Eliminado
                      && !movement.Eliminado
                      && !sale.Eliminado
                      && movement.Tipo == (short)Constantes.TipoTransaccionInventario.Salida
                      && movement.Glosa == SaleInventoryMovementDescription
                      && sale.Fecha >= selectedMonth
                      && sale.Fecha < selectedMonthEnd
                group detail by detail.IdProducto
                into productOutput
                select new ProductInventoryOutputSummary(
                    productOutput.Key,
                    productOutput.Sum(detail => detail.Cantidad)))
            .ToListAsync(cancellationToken);

        var costoVentas = CalculateCostOfGoodsSold(inventoryOutputs, productCosts);
        var utilidadBruta = ventasNetas - costoVentas;
        var margenBruto = ventasNetas == 0
            ? 0
            : Math.Round(utilidadBruta / ventasNetas * 100, 1);

        var productosRentables = BuildProfitableProducts(productSales, productCosts);

        var comprasPorProveedor = purchases
            .Where(purchase => purchase.Date >= selectedMonth && purchase.Date < selectedMonthEnd)
            .GroupBy(purchase => purchase.Supplier)
            .Select(group => new DashboardChartItemDTO
            {
                Etiqueta = group.Key,
                Valor = group.Sum(purchase => purchase.Total)
            })
            .OrderByDescending(item => item.Valor)
            .Take(RankingLimit)
            .ToList();

        var mesasMasUsadas = await _usoMesaRepository.Query()
            .Where(usage => !usage.Eliminado
                            && usage.Mesa != null
                            && usage.FechaInicio >= selectedMonth
                            && usage.FechaInicio < selectedMonthEnd)
            .GroupBy(usage => new { usage.IdMesa, usage.Mesa!.Nombre })
            .Select(group => new DashboardChartItemDTO
            {
                Etiqueta = group.Key.Nombre,
                Valor = group.Count()
            })
            .OrderByDescending(item => item.Valor)
            .Take(RankingLimit)
            .ToListAsync(cancellationToken);

        var dashboard = new DashboardDTO
        {
            FechaActualizacion = DateTime.Now,
            Mes = selectedMonth.Month,
            Anio = selectedMonth.Year,
            VentasNetas = ventasNetas,
            Compras = compras,
            CostoVentas = costoVentas,
            UtilidadBruta = utilidadBruta,
            MargenBruto = margenBruto,
            VentasVsCompras = BuildSalesVsPurchases(sales, purchases, periodStart),
            ProductosRentables = productosRentables,
            ComprasPorProveedor = comprasPorProveedor,
            MesasMasUsadas = mesasMasUsadas
        };

        return new Response<DashboardDTO>(dashboard);
    }

    private static decimal CalculateCostOfGoodsSold(
        IReadOnlyCollection<ProductInventoryOutputSummary> inventoryOutputs,
        IReadOnlyCollection<ProductCostSummary> productCosts)
    {
        var costsByProduct = productCosts.ToDictionary(cost => cost.ProductId);

        var costOfGoodsSold = inventoryOutputs.Sum(output =>
        {
            if (!costsByProduct.TryGetValue(output.ProductId, out var productCost)
                || productCost.BaseQuantity <= 0)
            {
                return 0;
            }

            var averageUnitCost = productCost.TotalCost / productCost.BaseQuantity;
            return (decimal)output.BaseQuantity * averageUnitCost;
        });

        return Math.Round(costOfGoodsSold, 2);
    }

    private static List<DashboardChartItemDTO> BuildProfitableProducts(
        IReadOnlyCollection<ProductSaleSummary> productSales,
        IReadOnlyCollection<ProductCostSummary> productCosts)
    {
        var costsByProduct = productCosts.ToDictionary(cost => cost.ProductId);

        return productSales
            .Select(productSale =>
            {
                var averageUnitCost = costsByProduct.TryGetValue(productSale.ProductId, out var productCost)
                                      && productCost.BaseQuantity > 0
                    ? productCost.TotalCost / productCost.BaseQuantity
                    : 0;

                return new DashboardChartItemDTO
                {
                    Etiqueta = productSale.ProductName,
                    Valor = Math.Round(
                        productSale.Revenue - productSale.BaseQuantity * averageUnitCost,
                        2)
                };
            })
            .Where(item => item.Valor > 0)
            .OrderByDescending(item => item.Valor)
            .Take(RankingLimit)
            .ToList();
    }

    private static List<DashboardComparisonItemDTO> BuildSalesVsPurchases(
        IReadOnlyCollection<SaleSummary> sales,
        IReadOnlyCollection<PurchaseSummary> purchases,
        DateTime periodStart)
    {
        return Enumerable.Range(0, MonthsToShow)
            .Select(offset =>
            {
                var month = periodStart.AddMonths(offset);
                return new DashboardComparisonItemDTO
                {
                    Etiqueta = SpanishCulture.TextInfo.ToTitleCase(month.ToString("MMM yy", SpanishCulture)),
                    Ventas = sales
                        .Where(sale => sale.Date.Year == month.Year && sale.Date.Month == month.Month)
                        .Sum(sale => sale.Total),
                    Compras = purchases
                        .Where(purchase => purchase.Date.Year == month.Year && purchase.Date.Month == month.Month)
                        .Sum(purchase => purchase.Total)
                };
            })
            .ToList();
    }

    private sealed record SaleSummary(DateTime Date, decimal Total);
    private sealed record PurchaseSummary(DateTime Date, decimal Total, string Supplier);
    private sealed record ProductSaleSummary(
        long ProductId,
        string ProductName,
        decimal BaseQuantity,
        decimal Revenue);
    private sealed record ProductCostSummary(long ProductId, decimal BaseQuantity, decimal TotalCost);
    private sealed record ProductInventoryOutputSummary(long ProductId, double BaseQuantity);
}
