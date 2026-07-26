using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using static Domain.Constants.Constantes;

namespace Application.Features.Inventory.TraspasoInventarios.Queries;

public class GetInventariosDisponiblesByAlmacenQuery : IQuery<Response<List<InventarioDTO>>>
{
    public required long IdAlmacen { get; set; }
}

public class GetInventariosDisponiblesByAlmacenHandler : IQueryHandler<GetInventariosDisponiblesByAlmacenQuery, Response<List<InventarioDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Inventario> _repository;

    public GetInventariosDisponiblesByAlmacenHandler(IMapper mapper, IRepository<Inventario> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<List<InventarioDTO>>> Handle(GetInventariosDisponiblesByAlmacenQuery request, CancellationToken cancellationToken)
    {
        var inventarios = await _repository.Query()
            .Where(inventario => !inventario.Eliminado
                && inventario.IdAlmacen == request.IdAlmacen
                && inventario.Cantidad > inventario.Reservado
                && inventario.Producto != null
                && !inventario.Producto.Eliminado
                && inventario.Producto.Activo
                && inventario.Producto.Tipo == (short)TipoProducto.Producto)
            .Include(inventario => inventario.Producto)
            .Include(inventario => inventario.Lote)
            .OrderBy(inventario => inventario.Producto!.Nombre)
            .ThenBy(inventario => inventario.Lote!.Codigo)
            .ToListAsync(cancellationToken);

        return new Response<List<InventarioDTO>>(_mapper.Map<List<InventarioDTO>>(inventarios));
    }
}
