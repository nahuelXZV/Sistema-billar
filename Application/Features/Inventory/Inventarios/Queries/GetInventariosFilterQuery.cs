using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;
using Domain.Entities.Inventory;
using Domain.Extensions;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.Inventarios.Queries;

public class GetInventariosFilterQuery : IQuery<Response<ResponseFilterDTO<InventarioDTO>>>
{
    public FilterDTO? Filter { get; set; }
}

public class GetInventariosFilterHandler : IQueryHandler<GetInventariosFilterQuery, Response<ResponseFilterDTO<InventarioDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Inventario> _repository;

    public GetInventariosFilterHandler(IMapper mapper, IRepository<Inventario> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<ResponseFilterDTO<InventarioDTO>>> Handle(GetInventariosFilterQuery request, CancellationToken cancellationToken)
    {
        var baseQuery = _repository.Query().Where(p => !p.Eliminado)
            .Include(p => p.Producto)
            .Include(p => p.Almacen)
            .Include(p => p.Lote);

        var total = await baseQuery.CountAsync(cancellationToken);

        var query = baseQuery.ApplyFilter(
                request.Filter,
                p => string.IsNullOrEmpty(request.Filter.Search)
                     || p.Producto.Nombre.ToLower().Contains(request.Filter.Search.ToLower())
                     || p.Almacen.Nombre.ToLower().Contains(request.Filter.Search.ToLower())
                     || p.Lote.Codigo.ToLower().Contains(request.Filter.Search.ToLower())
            );

        var listaInventario = await query.ToListAsync(cancellationToken);
        var listaInventarioDtos = _mapper.Map<List<InventarioDTO>>(listaInventario);

        var response = new ResponseFilterDTO<InventarioDTO>
        {
            Data = listaInventarioDtos,
            Total = total
        };
        return new Response<ResponseFilterDTO<InventarioDTO>>(response);
    }
}

