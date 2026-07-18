using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;
using Domain.Entities.Inventory;
using Domain.Extensions;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.Productos.Queries;

public class GetProductosFilterQuery : IQuery<Response<ResponseFilterDTO<ProductoDTO>>>
{
    public FilterDTO? Filter { get; set; }
}

public class GetProductosFilterQueryHandler : IQueryHandler<GetProductosFilterQuery, Response<ResponseFilterDTO<ProductoDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Producto> _repository;

    public GetProductosFilterQueryHandler(IMapper mapper, IRepository<Producto> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<ResponseFilterDTO<ProductoDTO>>> Handle(GetProductosFilterQuery request, CancellationToken cancellationToken)
    {
        var baseQuery = _repository.Query().Where(p => !p.Eliminado);

        var total = await baseQuery.CountAsync(cancellationToken);

        var query = baseQuery.ApplyFilter(
                request.Filter,
                p => string.IsNullOrEmpty(request.Filter.Search)
                     || p.Nombre.ToLower().Contains(request.Filter.Search.ToLower())
                     || p.Descripcion.ToLower().Contains(request.Filter.Search.ToLower())
            );

        var listaProductos = await query.ToListAsync(cancellationToken);
        var listaProductosDtos = _mapper.Map<List<ProductoDTO>>(listaProductos);

        var listaCategorias = await _repository.Query<Categoria>().Where(c => listaProductos.Select(p => p.IdCategoria).Contains(c.Id)).ToListAsync(cancellationToken);
        var listaUnidadesMedida = await _repository.Query<UnidadMedida>().Where(um => listaProductos.Select(p => p.IdUnidadMedida).Contains(um.Id)).ToListAsync(cancellationToken);

        foreach (var productoDto in listaProductosDtos)
        {
            productoDto.Categoria = listaCategorias.FirstOrDefault(c => c.Id == productoDto.IdCategoria);
            productoDto.UnidadMedida = listaUnidadesMedida.FirstOrDefault(um => um.Id == productoDto.IdUnidadMedida);
        }

        var response = new ResponseFilterDTO<ProductoDTO>
        {
            Data = listaProductosDtos,
            Total = total
        };

        return new Response<ResponseFilterDTO<ProductoDTO>>(response);
    }
}

