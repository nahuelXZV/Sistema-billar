using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sales.UsoMesas.Queries;

public class GetUsoMesasQuery : IQuery<Response<List<UsoMesaDTO>>>
{
}

public class GetUsoMesasQueryHandler : IQueryHandler<GetUsoMesasQuery, Response<List<UsoMesaDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<UsoMesa> _repository;

    public GetUsoMesasQueryHandler(IMapper mapper, IRepository<UsoMesa> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<List<UsoMesaDTO>>> Handle(GetUsoMesasQuery request, CancellationToken cancellationToken)
    {
        var listaUsoMesas = await _repository.Query()
            .Include(p => p.Mesa)
            .Where(p => !p.Eliminado)
            .ToListAsync(cancellationToken);

        var listaUsoMesasDto = _mapper.Map<List<UsoMesaDTO>>(listaUsoMesas);
        return new Response<List<UsoMesaDTO>>(listaUsoMesasDto);
    }
}
