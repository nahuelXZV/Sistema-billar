using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Configuration;
using Domain.DTOs.Shared;
using Domain.Entities.Configuration;
using Domain.Extensions;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Configuration.TipoMesas.Queries;

public class GetTipoMesaFilterQuery : ICommand<Response<ResponseFilterDTO<TipoMesaDTO>>>
{
    public FilterDTO? Filter { get; set; }
}

public class GetTipoMesaFilterQueryHandler : ICommandHandler<GetTipoMesaFilterQuery, Response<ResponseFilterDTO<TipoMesaDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<TipoMesa> _repository;

    public GetTipoMesaFilterQueryHandler(IMapper mapper, IRepository<TipoMesa> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<ResponseFilterDTO<TipoMesaDTO>>> Handle(GetTipoMesaFilterQuery request, CancellationToken cancellationToken)
    {
        var baseQuery = _repository.Query().Where(p => !p.Eliminado);

        var total = await baseQuery.CountAsync(cancellationToken);

        var query = baseQuery.ApplyFilter(
                request.Filter,
                p => string.IsNullOrEmpty(request.Filter.Search)
                     || p.Nombre.ToLower().Contains(request.Filter.Search.ToLower())
            );

        var listaTipos = await query.ToListAsync(cancellationToken);
        var listaTiposDtos = _mapper.Map<List<TipoMesaDTO>>(listaTipos);

        var response = new ResponseFilterDTO<TipoMesaDTO>
        {
            Data = listaTiposDtos,
            Total = total
        };

        return new Response<ResponseFilterDTO<TipoMesaDTO>>(response);
    }
}
