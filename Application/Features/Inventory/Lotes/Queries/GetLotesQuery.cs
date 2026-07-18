using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.Lotes.Queries;

public class GetLotesQuery : IQuery<Response<List<LoteDTO>>>
{
}

public class GetLotesHandler : IQueryHandler<GetLotesQuery, Response<List<LoteDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Lote> _repository;

    public GetLotesHandler(IMapper mapper, IRepository<Lote> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<List<LoteDTO>>> Handle(GetLotesQuery request, CancellationToken cancellationToken)
    {
        var inventario = await _repository.Query().Where(p => !p.Eliminado).ToListAsync();

        var inventarioDto = _mapper.Map<List<LoteDTO>>(inventario);

        return new Response<List<LoteDTO>>(inventarioDto);
    }
}

