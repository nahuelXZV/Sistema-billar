using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;
using Domain.Entities.Inventory;
using Domain.Extensions;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.TransaccionInventarios.Queries;

public class GetTransaccionInventarioFilterQuery : IQuery<Response<ResponseFilterDTO<TransaccionInventarioDetalleDTO>>>
{
    public FilterDTO? Filter { get; set; }
}

public class GetTransaccionInventarioFilterHandler : IQueryHandler<GetTransaccionInventarioFilterQuery, Response<ResponseFilterDTO<TransaccionInventarioDetalleDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<TransaccionInventarioDetalle> _repository;

    public GetTransaccionInventarioFilterHandler(IMapper mapper, IRepository<TransaccionInventarioDetalle> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<ResponseFilterDTO<TransaccionInventarioDetalleDTO>>> Handle(GetTransaccionInventarioFilterQuery request, CancellationToken cancellationToken)
    {
        var baseQuery = _repository.Query().Where(p => !p.Eliminado)
            .Include(p => p.Producto)
            .Include(p => p.Almacen)
            .Include(p => p.TransaccionInventario)
            .Include(p => p.Lote);

        var total = await baseQuery.CountAsync(cancellationToken);

        var query = baseQuery.ApplyFilter(
                request.Filter,
                p => string.IsNullOrEmpty(request.Filter.Search)
                     || p.IdTransaccion.ToString().Contains(request.Filter.Search)
                     || p.Producto.Nombre.ToLower().Contains(request.Filter.Search.ToLower())
                     || p.Almacen.Nombre.ToLower().Contains(request.Filter.Search.ToLower())
                     || (p.Lote != null && p.Lote.Codigo.ToLower().Contains(request.Filter.Search))
            );

        var listaInventario = await query.OrderByDescending(p => p.Id).ToListAsync(cancellationToken);
        var listaInventarioDtos = _mapper.Map<List<TransaccionInventarioDetalleDTO>>(listaInventario);

        var response = new ResponseFilterDTO<TransaccionInventarioDetalleDTO>
        {
            Data = listaInventarioDtos,
            Total = total
        };
        return new Response<ResponseFilterDTO<TransaccionInventarioDetalleDTO>>(response);
    }
}

