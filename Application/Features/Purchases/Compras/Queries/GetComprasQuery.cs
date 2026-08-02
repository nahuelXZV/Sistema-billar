using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Purchases;
using Domain.Entities.Purchases;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Purchases.Compras.Queries;

public class GetComprasQuery : IQuery<Response<List<CompraDTO>>>
{
}

public class GetComprasQueryHandler : IQueryHandler<GetComprasQuery, Response<List<CompraDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Compra> _repository;

    public GetComprasQueryHandler(IMapper mapper, IRepository<Compra> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<List<CompraDTO>>> Handle(GetComprasQuery request, CancellationToken cancellationToken)
    {
        var compras = await IncluirRelaciones(_repository.Query())
            .Where(compra => !compra.Eliminado)
            .ToListAsync(cancellationToken);

        return new Response<List<CompraDTO>>(_mapper.Map<List<CompraDTO>>(compras));
    }

    internal static IQueryable<Compra> IncluirRelaciones(IQueryable<Compra> query) =>
        query
            .Include(compra => compra.Proveedor)
            .Include(compra => compra.Almacen)
            .Include(compra => compra.Usuario)
            .Include(compra => compra.UsuarioAnulacion)
            .Include(compra => compra.TransaccionInventario)
            .Include(compra => compra.ListaDetalles.Where(detalle => !detalle.Eliminado))
                .ThenInclude(detalle => detalle.Producto)
            .Include(compra => compra.ListaDetalles.Where(detalle => !detalle.Eliminado))
                .ThenInclude(detalle => detalle.ProductoConversion)
                .ThenInclude(conversion => conversion!.UnidadMedida)
            .Include(compra => compra.ListaDetalles.Where(detalle => !detalle.Eliminado))
                .ThenInclude(detalle => detalle.Lote);
}
