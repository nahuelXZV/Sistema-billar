using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace Application.Features.Inventory.Lotes.Queries;

public class GetLoteByProductoQuery : ICommand<Response<List<LoteDTO>>>
{
    public required long IdProducto { get; set; }
}

public class GetLoteByProductoHandler : ICommandHandler<GetLoteByProductoQuery, Response<List<LoteDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Lote> _repository;

    public GetLoteByProductoHandler(IMapper mapper, IRepository<Lote> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<List<LoteDTO>>> Handle(GetLoteByProductoQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.Query()
            .Where(p => !p.Eliminado)
            .Where(p => p.IdProducto == request.IdProducto);

        var inventario = await query.ToListAsync();
        var inventarioDto = _mapper.Map<List<LoteDTO>>(inventario);

        return new Response<List<LoteDTO>>(inventarioDto);
    }
}

