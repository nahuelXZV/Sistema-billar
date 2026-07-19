using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sales.TurnosCaja.Queries;

public class GetTurnoCajaByIdQuery : IQuery<Response<TurnoCajaDTO>>
{
    public long Id { get; set; }
}

public class GetTurnoCajaByIdQueryHandler : IQueryHandler<GetTurnoCajaByIdQuery, Response<TurnoCajaDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<TurnoCaja> _repository;

    public GetTurnoCajaByIdQueryHandler(IMapper mapper, IRepository<TurnoCaja> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<TurnoCajaDTO>> Handle(GetTurnoCajaByIdQuery request, CancellationToken cancellationToken)
    {
        var turnoCaja = await _repository.Query()
            .Where(turno => turno.Id == request.Id && !turno.Eliminado)
            .Include(turno => turno.Vendedor)
            .Include(turno => turno.Detalles.Where(detalle => !detalle.Eliminado))
                .ThenInclude(detalle => detalle.MetodoPago)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ArgumentException("El turno de caja no existe.");

        return new Response<TurnoCajaDTO>(_mapper.Map<TurnoCajaDTO>(turnoCaja));
    }
}
