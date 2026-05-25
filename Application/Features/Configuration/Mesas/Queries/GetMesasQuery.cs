using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Configuration;
using Domain.Entities.Configuration;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Configuration.Mesas.Queries;

public class GetMesasQuery : ICommand<Response<List<MesaDTO>>>
{
}

public class GetMesasQueryHandler : ICommandHandler<GetMesasQuery, Response<List<MesaDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Mesa> _repository;

    public GetMesasQueryHandler(IMapper mapper, IRepository<Mesa> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<List<MesaDTO>>> Handle(GetMesasQuery request, CancellationToken cancellationToken)
    {
        var listaMesas = await _repository.Query().Where(p => !p.Eliminado).ToListAsync(cancellationToken);
        var listaMesasDtos = _mapper.Map<List<MesaDTO>>(listaMesas);
        return new Response<List<MesaDTO>>(listaMesasDtos);
    }
}
