using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.Productos.Queries;

public class GetProductoByIdQuery : IQuery<Response<ProductoDTO>>
{
    public required long Id { get; set; }
}

public class GetProductoByIdQueryHandler : IQueryHandler<GetProductoByIdQuery, Response<ProductoDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Producto> _repository;
    private readonly IRepository<ProductoCompuesto> _rpProductoCompuesto;
    private readonly IRepository<ProductoConversion> _rpProductoConversion;

    public GetProductoByIdQueryHandler(
        IMapper mapper,
        IRepository<Producto> repository,
        IRepository<ProductoCompuesto> rpProductoCompuesto,
        IRepository<ProductoConversion> rpProductoConversion)
    {
        _mapper = mapper;
        _repository = repository;
        _rpProductoCompuesto = rpProductoCompuesto;
        _rpProductoConversion = rpProductoConversion;
    }

    public async Task<Response<ProductoDTO>> Handle(GetProductoByIdQuery request, CancellationToken cancellationToken)
    {
        var producto = await _repository.Query()
            .Where(p => !p.Eliminado)
            .Where(p => p.Id == request.Id)
            .FirstOrDefaultAsync();

        if (producto == null) throw new Exception("Producto no encontrado.");
        var productoDto = _mapper.Map<ProductoDTO>(producto);

        if (producto.EsCompuesto)
        {
            var listaCompuesto = await _rpProductoCompuesto.Query()
                .Where(pc => pc.IdProductoPadre == producto.Id)
                .Where(pc => !pc.Eliminado)
                .ToListAsync(cancellationToken);

            if (listaCompuesto != null && listaCompuesto.Any())
            {
                var listaProductosIds = listaCompuesto.Select(pc => pc.IdProductoComponente).ToList();
                var listaProductos = await _repository.Query()
                    .Where(p => listaProductosIds.Contains(p.Id))
                    .Where(p => !p.Eliminado)
                    .ToListAsync(cancellationToken);

                productoDto.ProductosCompuestos = listaCompuesto.Select(pc =>
                {
                    Producto? productoComponente = listaProductos.FirstOrDefault(p => p.Id == pc.IdProductoComponente);
                    return new ProductoCompuestoDTO
                    {
                        IdProductoComponente = pc.IdProductoComponente,
                        Cantidad = pc.Cantidad,
                        ProductoComponente = _mapper.Map<ProductoDTO>(productoComponente)
                    };
                }).ToList();
            }
        }

        var conversiones = await _rpProductoConversion.Query()
            .Include(conversion => conversion.UnidadMedida)
            .Where(conversion => conversion.IdProducto == producto.Id && !conversion.Eliminado)
            .ToListAsync(cancellationToken);

        productoDto.ProductoConversiones = _mapper.Map<List<ProductoConversionDTO>>(conversiones);

        return new Response<ProductoDTO>(productoDto);
    }
}

