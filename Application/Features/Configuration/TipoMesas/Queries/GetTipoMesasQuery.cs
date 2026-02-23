using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Configuration;
using Domain.Entities.Configuration;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace Application.Features.Configuration.TipoMesas.Queries;

public class GetTipoMesasQuery : ICommand<Response<List<TipoMesaDTO>>>
{
}

public class GetTipoMesasQueryHandler : ICommandHandler<GetTipoMesasQuery, Response<List<TipoMesaDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<TipoMesa> _repository;

    public GetTipoMesasQueryHandler(IMapper mapper, IRepository<TipoMesa> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<List<TipoMesaDTO>>> Handle(GetTipoMesasQuery request, CancellationToken cancellationToken)
    {
        var listaTipos = await _repository.Query().Where(p => !p.Eliminado).ToListAsync(cancellationToken);
        var listaTiposDtos = _mapper.Map<List<TipoMesaDTO>>(listaTipos);
        return new Response<List<TipoMesaDTO>>(listaTiposDtos);
    }
}
