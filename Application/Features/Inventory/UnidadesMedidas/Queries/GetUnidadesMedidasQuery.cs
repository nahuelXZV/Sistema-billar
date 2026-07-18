using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.UnidadesMedidas.Queries;

public class GetUnidadesMedidasQuery : IQuery<Response<List<UnidadMedidaDTO>>>
{
}

public class GetUnidadesMedidasQueryHandler : IQueryHandler<GetUnidadesMedidasQuery, Response<List<UnidadMedidaDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<UnidadMedida> _repository;

    public GetUnidadesMedidasQueryHandler(IMapper mapper, IRepository<UnidadMedida> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<List<UnidadMedidaDTO>>> Handle(GetUnidadesMedidasQuery request, CancellationToken cancellationToken)
    {
        var listaUnidades = await _repository.Query().Where(p => !p.Eliminado).ToListAsync(cancellationToken);
        var listaUnidadesDtos = _mapper.Map<List<UnidadMedidaDTO>>(listaUnidades);
        return new Response<List<UnidadMedidaDTO>>(listaUnidadesDtos);
    }
}

