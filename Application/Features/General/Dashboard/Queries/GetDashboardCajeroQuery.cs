using Application.Interfaces;
using Domain.Common;
using Domain.DTOs.General;
using Domain.Entities.Configuration;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using static Domain.Constants.Constantes;

namespace Application.Features.General.Dashboard.Queries;

public class GetDashboardCajeroQuery : IQuery<Response<DashboardCajeroDTO>>
{
    public long IdUsuario { get; set; }
}

public class GetDashboardCajeroQueryHandler
    : IQueryHandler<GetDashboardCajeroQuery, Response<DashboardCajeroDTO>>
{
    private readonly IRepository<Vendedor> _vendedorRepository;
    private readonly IRepository<TurnoCaja> _turnoCajaRepository;
    private readonly IRepository<Venta> _ventaRepository;
    private readonly IRepository<PagoVenta> _pagoVentaRepository;
    private readonly IRepository<Mesa> _mesaRepository;
    private readonly IRepository<UsoMesa> _usoMesaRepository;

    public GetDashboardCajeroQueryHandler(
        IRepository<Vendedor> vendedorRepository,
        IRepository<TurnoCaja> turnoCajaRepository,
        IRepository<Venta> ventaRepository,
        IRepository<PagoVenta> pagoVentaRepository,
        IRepository<Mesa> mesaRepository,
        IRepository<UsoMesa> usoMesaRepository)
    {
        _vendedorRepository = vendedorRepository;
        _turnoCajaRepository = turnoCajaRepository;
        _ventaRepository = ventaRepository;
        _pagoVentaRepository = pagoVentaRepository;
        _mesaRepository = mesaRepository;
        _usoMesaRepository = usoMesaRepository;
    }

    public async Task<Response<DashboardCajeroDTO>> Handle(
        GetDashboardCajeroQuery request,
        CancellationToken cancellationToken)
    {
        if (request.IdUsuario <= 0)
        {
            throw new UnauthorizedAccessException("No se pudo identificar al usuario autenticado.");
        }

        var vendedor = await _vendedorRepository.Query()
            .Where(item =>
                item.IdUsuario == request.IdUsuario &&
                item.Activo &&
                !item.Eliminado)
            .Select(item => new { item.Id, item.Nombre })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException(
                "El usuario no tiene un vendedor activo asignado.");

        var dashboard = new DashboardCajeroDTO
        {
            FechaActualizacion = DateTime.Now,
            NombreVendedor = vendedor.Nombre
        };

        await CargarEstadoMesasAsync(dashboard, cancellationToken);

        var turno = await _turnoCajaRepository.Query()
            .Where(item =>
                item.IdVendedor == vendedor.Id &&
                item.Estado == (short)EstadoTurnoCaja.Abierto &&
                !item.Eliminado)
            .Include(item => item.Detalles.Where(detalle => !detalle.Eliminado))
                .ThenInclude(detalle => detalle.MetodoPago)
            .OrderByDescending(item => item.FechaApertura)
            .FirstOrDefaultAsync(cancellationToken);

        if (turno is null)
        {
            return new Response<DashboardCajeroDTO>(dashboard);
        }

        dashboard.TieneTurnoActivo = true;
        dashboard.IdTurnoCaja = turno.Id;
        dashboard.FechaApertura = turno.FechaApertura;

        var resumenVentas = await _ventaRepository.Query()
            .Where(venta =>
                venta.IdTurnoCaja == turno.Id &&
                venta.IdVendedor == vendedor.Id &&
                !venta.Eliminado)
            .GroupBy(_ => 1)
            .Select(grupo => new
            {
                Cantidad = grupo.Count(),
                Total = grupo.Sum(venta => venta.Total)
            })
            .FirstOrDefaultAsync(cancellationToken);

        dashboard.CantidadVentasTurno = resumenVentas?.Cantidad ?? 0;
        dashboard.TotalVendidoTurno = resumenVentas?.Total ?? 0;

        var ventasPorMetodo = await _pagoVentaRepository.Query()
            .Where(pago =>
                !pago.Eliminado &&
                pago.Venta != null &&
                !pago.Venta.Eliminado &&
                pago.Venta.IdTurnoCaja == turno.Id &&
                pago.Venta.IdVendedor == vendedor.Id)
            .GroupBy(pago => pago.IdMetodoPago)
            .Select(grupo => new
            {
                IdMetodoPago = grupo.Key,
                Monto = grupo.Sum(pago => pago.MontoTotal)
            })
            .ToDictionaryAsync(item => item.IdMetodoPago, item => item.Monto, cancellationToken);

        dashboard.VentasPorMetodoPago = turno.Detalles
            .Where(detalle => detalle.MetodoPago is not null)
            .Select(detalle =>
            {
                var metodo = detalle.MetodoPago!;
                var montoVendido = ventasPorMetodo.GetValueOrDefault(detalle.IdMetodoPago);
                var esEfectivo = EsMetodoEfectivo(
                    metodo.Nombre,
                    metodo.Abreviatura);

                return new DashboardMetodoPagoDTO
                {
                    IdMetodoPago = detalle.IdMetodoPago,
                    Nombre = metodo.Nombre,
                    Icono = metodo.Icono,
                    MontoApertura = detalle.MontoApertura,
                    MontoVendido = montoVendido,
                    MontoEsperado = detalle.MontoApertura + montoVendido,
                    EsEfectivo = esEfectivo
                };
            })
            .OrderByDescending(item => item.MontoVendido)
            .ThenBy(item => item.Nombre)
            .ToList();

        dashboard.VentasEfectivo = dashboard.VentasPorMetodoPago
            .Where(item => item.EsEfectivo)
            .Sum(item => item.MontoVendido);
        dashboard.PagosDigitales = dashboard.VentasPorMetodoPago
            .Where(item => !item.EsEfectivo)
            .Sum(item => item.MontoVendido);
        dashboard.EfectivoEsperado = dashboard.VentasPorMetodoPago
            .Where(item => item.EsEfectivo)
            .Sum(item => item.MontoEsperado);

        return new Response<DashboardCajeroDTO>(dashboard);
    }

    private async Task CargarEstadoMesasAsync(
        DashboardCajeroDTO dashboard,
        CancellationToken cancellationToken)
    {
        var idsMesasActivas = await _mesaRepository.Query()
            .Where(mesa => mesa.Activo && !mesa.Eliminado)
            .Select(mesa => mesa.Id)
            .ToListAsync(cancellationToken);

        if (idsMesasActivas.Count == 0)
        {
            return;
        }

        var estadosMesasConOrden = await _usoMesaRepository.Query()
            .Where(uso =>
                !uso.Eliminado &&
                idsMesasActivas.Contains(uso.IdMesa) &&
                uso.OrdenVenta != null &&
                !uso.OrdenVenta.Eliminado &&
                uso.OrdenVenta.Estado == (short)EstadoOrdenVenta.Abierta)
            .Select(uso => new { uso.IdMesa, uso.Estado })
            .ToListAsync(cancellationToken);

        var ultimoEstadoPorMesa = estadosMesasConOrden
            .GroupBy(item => item.IdMesa)
            .Select(grupo => grupo.OrderByDescending(item => item.Estado).First())
            .ToList();

        dashboard.MesasPorCobrar = ultimoEstadoPorMesa.Count(
            item => item.Estado == (short)EstadoUsoMesa.Finalizado);
        dashboard.MesasOcupadas = ultimoEstadoPorMesa.Count - dashboard.MesasPorCobrar;
        dashboard.MesasDisponibles = Math.Max(
            0,
            idsMesasActivas.Count - ultimoEstadoPorMesa.Count);
    }

    private static bool EsMetodoEfectivo(string nombre, string abreviatura)
    {
        return ContieneEfectivo(nombre) || ContieneEfectivo(abreviatura);
    }

    private static bool ContieneEfectivo(string value)
    {
        return value.Contains("efectivo", StringComparison.OrdinalIgnoreCase)
            || value.Contains("cash", StringComparison.OrdinalIgnoreCase);
    }
}
