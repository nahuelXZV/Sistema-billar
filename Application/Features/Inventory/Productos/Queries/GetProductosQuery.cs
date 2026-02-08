using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.Productos.Queries;

public class GetProductosQuery : ICommand<Response<List<ProductoDTO>>>
{
}

public class GetProductosQueryHandler : ICommandHandler<GetProductosQuery, Response<List<ProductoDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Producto> _repository;

    public GetProductosQueryHandler(IMapper mapper, IRepository<Producto> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<List<ProductoDTO>>> Handle(GetProductosQuery request, CancellationToken cancellationToken)
    {
        var listaProductos = await _repository.Query().Where(p => !p.Eliminado).ToListAsync(cancellationToken);
        var listaProductosDtos = _mapper.Map<List<ProductoDTO>>(listaProductos);
        return new Response<List<ProductoDTO>>(listaProductosDtos);
    }
}
