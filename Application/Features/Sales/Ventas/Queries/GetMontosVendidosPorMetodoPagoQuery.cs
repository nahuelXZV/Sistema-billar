using Application.Interfaces;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sales.Ventas.Queries;

public class GetMontosVendidosPorMetodoPagoQuery
    : IQuery<Response<List<VentaMetodoPagoTotalDTO>>>
{
    public long IdVendedor { get; set; }
    public long IdTurnoCaja { get; set; }
}

public class GetMontosVendidosPorMetodoPagoQueryHandler
    : IQueryHandler<GetMontosVendidosPorMetodoPagoQuery, Response<List<VentaMetodoPagoTotalDTO>>>
{
    private readonly IRepository<PagoVenta> _pagoVentaRepository;

    public GetMontosVendidosPorMetodoPagoQueryHandler(IRepository<PagoVenta> pagoVentaRepository)
    {
        _pagoVentaRepository = pagoVentaRepository;
    }

    public async Task<Response<List<VentaMetodoPagoTotalDTO>>> Handle(
        GetMontosVendidosPorMetodoPagoQuery request,
        CancellationToken cancellationToken)
    {
        if (request.IdVendedor <= 0)
            throw new ArgumentException("El vendedor no es válido.");

        if (request.IdTurnoCaja <= 0)
            throw new ArgumentException("El turno de caja no es válido.");

        var montos = await _pagoVentaRepository.Query()
            .Where(pago =>
                !pago.Eliminado &&
                pago.Venta != null &&
                !pago.Venta.Eliminado &&
                pago.Venta.IdVendedor == request.IdVendedor &&
                pago.Venta.IdTurnoCaja == request.IdTurnoCaja)
            .GroupBy(pago => pago.IdMetodoPago)
            .Select(grupo => new VentaMetodoPagoTotalDTO
            {
                IdMetodoPago = grupo.Key,
                MontoVendido = grupo.Sum(pago => pago.MontoTotal)
            })
            .ToListAsync(cancellationToken);

        return new Response<List<VentaMetodoPagoTotalDTO>>(montos);
    }
}
