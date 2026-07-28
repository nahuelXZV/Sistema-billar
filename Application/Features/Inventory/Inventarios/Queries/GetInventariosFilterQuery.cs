using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;
using Domain.Entities.Inventory;
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
        IQueryable<Inventario> baseQuery = _repository.Query()
            .AsNoTracking()
            .Where(p => !p.Eliminado)
            .Include(p => p.Producto)
            .Include(p => p.Almacen)
            .Include(p => p.Lote);

        var filteredQuery = baseQuery;
        var search = request.Filter?.Search?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.ToLower();
            var idsUnidadesMedida = await _repository.Query<UnidadMedida>()
                .AsNoTracking()
                .Where(unidadMedida => !unidadMedida.Eliminado && (unidadMedida.Nombre.ToLower().Contains(normalizedSearch) || unidadMedida.Abreviatura.ToLower().Contains(normalizedSearch)))
                .Select(unidadMedida => unidadMedida.Id)
                .ToListAsync(cancellationToken);

            filteredQuery = filteredQuery.Where(inventario => inventario.Producto!.Nombre.ToLower().Contains(normalizedSearch) ||
                inventario.Almacen!.Nombre.ToLower().Contains(normalizedSearch) ||
                (inventario.Lote != null && inventario.Lote.Codigo.ToLower().Contains(normalizedSearch)) ||
                idsUnidadesMedida.Contains(inventario.Producto.IdUnidadMedida));
        }

        var total = await filteredQuery.CountAsync(cancellationToken);

        var orderedQuery = filteredQuery
            .OrderBy(inventario => inventario.Producto!.Nombre)
            .ThenBy(inventario => inventario.Almacen!.Nombre)
            .ThenBy(inventario => inventario.Lote != null ? inventario.Lote.Codigo : string.Empty)
            .ThenBy(inventario => inventario.Id);

        IQueryable<Inventario> query = orderedQuery;
        if (request.Filter?.Offset > 0)
        {
            query = query.Skip(request.Filter.Offset);
        }

        if (request.Filter?.Limit > 0)
        {
            query = query.Take(request.Filter.Limit);
        }

        var listaInventario = await query.ToListAsync(cancellationToken);
        var listaInventarioDtos = _mapper.Map<List<InventarioDTO>>(listaInventario);
        var idsUnidadesBase = listaInventario
            .Where(inventario => inventario.Producto != null)
            .Select(inventario => inventario.Producto!.IdUnidadMedida)
            .Distinct()
            .ToList();

        var unidadesMedida = idsUnidadesBase.Count == 0 ? new List<UnidadMedida>()
            : await _repository.Query<UnidadMedida>()
                .AsNoTracking()
                .Where(unidadMedida => !unidadMedida.Eliminado && idsUnidadesBase.Contains(unidadMedida.Id))
                .ToListAsync(cancellationToken);

        var unidadesMedidaPorId = unidadesMedida.ToDictionary(unidadMedida => unidadMedida.Id);
        foreach (var inventarioDto in listaInventarioDtos)
        {
            if (inventarioDto.Producto != null && unidadesMedidaPorId.TryGetValue(inventarioDto.Producto.IdUnidadMedida, out var unidadMedida))
            {
                inventarioDto.Producto.UnidadMedida = unidadMedida;
            }
        }

        var response = new ResponseFilterDTO<InventarioDTO>
        {
            Data = listaInventarioDtos,
            Total = total
        };
        return new Response<ResponseFilterDTO<InventarioDTO>>(response);
    }
}

