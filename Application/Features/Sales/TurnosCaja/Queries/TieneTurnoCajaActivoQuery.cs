using Application.Interfaces;
using Domain.Common;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using static Domain.Constants.Constantes;

namespace Application.Features.Sales.TurnosCaja.Queries;

public class TieneTurnoCajaActivoQuery : IQuery<Response<bool>>
{
    public long IdVendedor { get; set; }
}

public class TieneTurnoCajaActivoQueryHandler
    : IQueryHandler<TieneTurnoCajaActivoQuery, Response<bool>>
{
    private readonly IRepository<TurnoCaja> _turnoCajaRepository;

    public TieneTurnoCajaActivoQueryHandler(IRepository<TurnoCaja> turnoCajaRepository)
    {
        _turnoCajaRepository = turnoCajaRepository;
    }

    public async Task<Response<bool>> Handle(
        TieneTurnoCajaActivoQuery request,
        CancellationToken cancellationToken)
    {
        if (request.IdVendedor <= 0)
            return new Response<bool>(false);

        var tieneTurnoActivo = await _turnoCajaRepository.Query()
            .AnyAsync(turno =>
                turno.IdVendedor == request.IdVendedor &&
                turno.Estado == (short)EstadoTurnoCaja.Abierto &&
                !turno.Eliminado,
                cancellationToken);

        return new Response<bool>(tieneTurnoActivo);
    }
}
