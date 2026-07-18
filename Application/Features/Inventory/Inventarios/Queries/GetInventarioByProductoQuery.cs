using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.Inventarios.Queries;

public class GetInventarioByProductoQuery : IQuery<Response<List<InventarioDTO>>>
{
    public required long IdProducto { get; set; }
}

public class GetInventarioByProductoHandler : IQueryHandler<GetInventarioByProductoQuery, Response<List<InventarioDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Inventario> _repository;

    public GetInventarioByProductoHandler(IMapper mapper, IRepository<Inventario> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<List<InventarioDTO>>> Handle(GetInventarioByProductoQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.Query()
            .Include(p => p.Lote)
            .Include(p => p.Almacen)
            .Include(p => p.Producto)
            .Where(p => !p.Eliminado)
            .Where(p => p.IdProducto == request.IdProducto);

        var inventario = await query.ToListAsync();
        if (inventario == null) throw new Exception("Inventario no encontrado.");

        var inventarioDto = _mapper.Map<List<InventarioDTO>>(inventario);
        return new Response<List<InventarioDTO>>(inventarioDto);
    }
}

