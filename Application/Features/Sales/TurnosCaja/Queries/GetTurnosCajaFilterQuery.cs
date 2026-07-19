using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.DTOs.Shared;
using Domain.Entities.Sales;
using Domain.Extensions;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sales.TurnosCaja.Queries;

public class GetTurnosCajaFilterQuery : IQuery<Response<ResponseFilterDTO<TurnoCajaDTO>>>
{
    public FilterDTO? Filter { get; set; }
}

public class GetTurnosCajaFilterQueryHandler : IQueryHandler<GetTurnosCajaFilterQuery, Response<ResponseFilterDTO<TurnoCajaDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<TurnoCaja> _repository;

    public GetTurnosCajaFilterQueryHandler(IMapper mapper, IRepository<TurnoCaja> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<ResponseFilterDTO<TurnoCajaDTO>>> Handle(GetTurnosCajaFilterQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.Query()
            .Where(turno => !turno.Eliminado)
            .Include(turno => turno.Vendedor)
            .Include(turno => turno.Detalles.Where(detalle => !detalle.Eliminado))
                .ThenInclude(detalle => detalle.MetodoPago)
            .AsQueryable();

        var search = request.Filter?.Search;
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(turno =>
                turno.Observacion.Contains(search) ||
                (turno.Vendedor != null &&
                    (turno.Vendedor.Nombre.Contains(search) || turno.Vendedor.Documento.Contains(search))));
        }

        var total = await query.CountAsync(cancellationToken);
        var turnosCaja = await query
            .OrderByDescending(turno => turno.Id)
            .ApplyFilter(request.Filter)
            .ToListAsync(cancellationToken);

        return new Response<ResponseFilterDTO<TurnoCajaDTO>>(new ResponseFilterDTO<TurnoCajaDTO>
        {
            Data = _mapper.Map<List<TurnoCajaDTO>>(turnosCaja),
            Total = total
        });
    }
}
