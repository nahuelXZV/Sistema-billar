using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.Productos.Queries;

public class GetProductosByCategoriaQuery : IQuery<Response<List<ProductoDTO>>>
{
    public long IdCategoria { get; set; }
    public long IdVendedor { get; set; }
}

public class GetProductosByCategoriaHandler : IQueryHandler<GetProductosByCategoriaQuery, Response<List<ProductoDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Producto> _repository;

    public GetProductosByCategoriaHandler(IMapper mapper, IRepository<Producto> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<List<ProductoDTO>>> Handle(GetProductosByCategoriaQuery request, CancellationToken cancellationToken)
    {
        var vendedor = await _repository.Query<Vendedor>()
            .Where(v => !v.Eliminado && v.Activo && v.Id == request.IdVendedor)
            .FirstOrDefaultAsync(cancellationToken);

        if (vendedor is null || vendedor.IdListaPrecio <= 0)
        {
            return new Response<List<ProductoDTO>>([]);
        }

        var detallesPrecio = await _repository.Query<ListaPreciosDetalle>()
            .Include(detalle => detalle.ProductoConversion).ThenInclude(conversion => conversion!.UnidadMedida)
            .Where(detalle =>
                !detalle.Eliminado &&
                detalle.IdListaPrecio == vendedor.IdListaPrecio &&
                detalle.ProductoConversion != null &&
                !detalle.ProductoConversion.Eliminado)
            .ToListAsync(cancellationToken);

        if (detallesPrecio.Count == 0)
        {
            return new Response<List<ProductoDTO>>([]);
        }

        var productosConPrecioIds = detallesPrecio
            .Select(detalle => detalle.ProductoConversion!.IdProducto)
            .Distinct()
            .ToList();

        var productos = await _repository.Query()
            .Where(p => !p.Eliminado
                && p.Activo
                && p.IdCategoria == request.IdCategoria
                && productosConPrecioIds.Contains(p.Id))
            .OrderBy(p => p.Nombre)
            .ToListAsync(cancellationToken);

        var productosDto = _mapper.Map<List<ProductoDTO>>(productos);

        foreach (var productoDto in productosDto)
        {
            productoDto.PreciosVenta = detallesPrecio
                .Where(detalle => detalle.ProductoConversion!.IdProducto == productoDto.Id)
                .OrderBy(detalle => detalle.ProductoConversion!.IdUnidadMedida == productoDto.IdUnidadMedida ? 0 : 1)
                .ThenBy(detalle => detalle.ProductoConversion!.FactorConversion)
                .Select(detalle => new ProductoPrecioVentaDTO
                {
                    IdProductoConversion = detalle.IdProductoConversion,
                    IdUnidadMedida = detalle.ProductoConversion!.IdUnidadMedida,
                    NombreUnidadMedida = detalle.ProductoConversion.UnidadMedida?.Nombre ?? string.Empty,
                    AbreviaturaUnidadMedida = detalle.ProductoConversion.UnidadMedida?.Abreviatura ?? string.Empty,
                    FactorConversion = detalle.ProductoConversion.FactorConversion,
                    Precio = detalle.Precio,
                    EsUnidadBase = detalle.ProductoConversion.IdUnidadMedida == productoDto.IdUnidadMedida
                })
                .ToList();

            productoDto.Precio = productoDto.PreciosVenta.FirstOrDefault()?.Precio ?? 0m;
        }

        return new Response<List<ProductoDTO>>(productosDto);
    }
}
