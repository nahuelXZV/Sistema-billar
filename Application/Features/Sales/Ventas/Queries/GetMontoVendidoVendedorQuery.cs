using Application.Interfaces;
using Domain.Common;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sales.Ventas.Queries;

public class GetMontoVendidoVendedorQuery : IQuery<Response<decimal>>
{
    public long IdVendedor { get; set; }
    public long IdTurnoCaja { get; set; }
}

public class GetMontoVendidoVendedorQueryHandler
    : IQueryHandler<GetMontoVendidoVendedorQuery, Response<decimal>>
{
    private readonly IRepository<Venta> _ventaRepository;

    public GetMontoVendidoVendedorQueryHandler(IRepository<Venta> ventaRepository)
    {
        _ventaRepository = ventaRepository;
    }

    public async Task<Response<decimal>> Handle(
        GetMontoVendidoVendedorQuery request,
        CancellationToken cancellationToken)
    {
        if (request.IdVendedor <= 0)
            throw new ArgumentException("El vendedor no es válido.");

        if (request.IdTurnoCaja <= 0)
            throw new ArgumentException("El turno de caja no es válido.");

        var montoVendido = await _ventaRepository.Query()
            .Where(venta =>
                venta.IdVendedor == request.IdVendedor &&
                venta.IdTurnoCaja == request.IdTurnoCaja &&
                !venta.Eliminado)
            .SumAsync(venta => (decimal?)venta.Total, cancellationToken) ?? 0;

        return new Response<decimal>(montoVendido);
    }
}
