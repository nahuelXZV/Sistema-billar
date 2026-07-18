using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;
using Domain.Entities.Inventory;
using Domain.Extensions;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.UnidadesMedidas.Queries;

public class GetUnidadesMedidasFilterQuery : IQuery<Response<ResponseFilterDTO<UnidadMedidaDTO>>>
{
    public FilterDTO? Filter { get; set; }
}

public class GetUnidadesMedidasFilterQueryHandler : IQueryHandler<GetUnidadesMedidasFilterQuery, Response<ResponseFilterDTO<UnidadMedidaDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<UnidadMedida> _repository;

    public GetUnidadesMedidasFilterQueryHandler(IMapper mapper, IRepository<UnidadMedida> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<ResponseFilterDTO<UnidadMedidaDTO>>> Handle(GetUnidadesMedidasFilterQuery request, CancellationToken cancellationToken)
    {
        var baseQuery = _repository.Query().Where(p => !p.Eliminado);

        var total = await baseQuery.CountAsync(cancellationToken);

        var query = baseQuery.ApplyFilter(
                request.Filter,
                p => string.IsNullOrEmpty(request.Filter.Search)
                     || p.Nombre.ToLower().Contains(request.Filter.Search.ToLower())
                     || p.Abreviatura.ToLower().Contains(request.Filter.Search.ToLower())
            );

        var listaUnidades = await query.ToListAsync(cancellationToken);
        var listaUnidadesDtos = _mapper.Map<List<UnidadMedidaDTO>>(listaUnidades);

        var response = new ResponseFilterDTO<UnidadMedidaDTO>
        {
            Data = listaUnidadesDtos,
            Total = total
        };

        return new Response<ResponseFilterDTO<UnidadMedidaDTO>>(response);
    }
}

