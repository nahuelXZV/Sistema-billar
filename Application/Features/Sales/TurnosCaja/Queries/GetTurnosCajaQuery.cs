using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sales.TurnosCaja.Queries;

public class GetTurnosCajaQuery : IQuery<Response<List<TurnoCajaDTO>>>
{
}

public class GetTurnosCajaQueryHandler : IQueryHandler<GetTurnosCajaQuery, Response<List<TurnoCajaDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<TurnoCaja> _repository;

    public GetTurnosCajaQueryHandler(IMapper mapper, IRepository<TurnoCaja> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<List<TurnoCajaDTO>>> Handle(GetTurnosCajaQuery request, CancellationToken cancellationToken)
    {
        var turnosCaja = await _repository.Query()
            .Where(turno => !turno.Eliminado)
            .Include(turno => turno.Vendedor)
            .Include(turno => turno.Detalles.Where(detalle => !detalle.Eliminado))
                .ThenInclude(detalle => detalle.MetodoPago)
            .OrderByDescending(turno => turno.Id)
            .ToListAsync(cancellationToken);

        return new Response<List<TurnoCajaDTO>>(_mapper.Map<List<TurnoCajaDTO>>(turnosCaja));
    }
}
