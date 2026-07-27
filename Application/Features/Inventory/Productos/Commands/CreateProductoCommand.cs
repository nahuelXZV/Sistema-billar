using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
namespace Application.Features.Inventory.Productos.Commands;

public class CreateProductoCommand : ICommand<Response<long>>
{
    public required ProductoDTO ProductoDTO { get; set; }
}

public class CreateProductoCommandHandler : ICommandHandler<CreateProductoCommand, Response<long>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Producto> _repository;
    private readonly IRepository<ProductoCompuesto> _rpProductoCompuesto;
    private readonly IRepository<ProductoConversion> _rpProductoConversion;

    public CreateProductoCommandHandler(
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

    public async Task<Response<long>> Handle(CreateProductoCommand request, CancellationToken cancellationToken)
    {
        var conversiones = NormalizarConversiones(request.ProductoDTO);
        var producto = _mapper.Map<Producto>(request.ProductoDTO);

        producto.FechaCreacion = DateTime.Now;
        producto = await _repository.AddAsync(producto);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        if (producto.EsCompuesto)
        {
            var productosCompuestos = request.ProductoDTO.ProductosCompuestos?.Select(pc => new ProductoCompuesto
            {
                IdProductoPadre = producto.Id,
                IdProductoComponente = pc.IdProductoComponente,
                Cantidad = pc.Cantidad
            }).ToList();

            if (productosCompuestos != null && productosCompuestos.Any())
                await _rpProductoCompuesto.AddRangeAsync(productosCompuestos);
        }

        await _rpProductoConversion.AddRangeAsync(conversiones.Select(conversion => new ProductoConversion
        {
            IdProducto = producto.Id,
            IdUnidadMedida = conversion.IdUnidadMedida,
            FactorConversion = conversion.FactorConversion
        }));

        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<long>(producto.Id);
    }

    private static List<ProductoConversionDTO> NormalizarConversiones(ProductoDTO producto)
    {
        if (producto.IdUnidadMedida <= 0)
            throw new InvalidOperationException("El producto debe tener una unidad de medida base.");

        var conversiones = producto.ProductoConversiones?.ToList() ?? [];

        if (conversiones.Any(conversion => conversion.IdUnidadMedida <= 0))
            throw new InvalidOperationException("Todas las conversiones deben tener una unidad de medida.");

        if (conversiones.Any(conversion => conversion.FactorConversion <= 0))
            throw new InvalidOperationException("Todos los factores de conversión deben ser mayores a cero.");

        if (conversiones
            .GroupBy(conversion => conversion.IdUnidadMedida)
            .Any(grupo => grupo.Count() > 1))
        {
            throw new InvalidOperationException("No se puede repetir una unidad de medida en las conversiones del producto.");
        }

        var unidadBase = conversiones.FirstOrDefault(
            conversion => conversion.IdUnidadMedida == producto.IdUnidadMedida);

        if (unidadBase == null)
        {
            conversiones.Add(new ProductoConversionDTO
            {
                IdUnidadMedida = producto.IdUnidadMedida,
                FactorConversion = 1
            });
        }
        else
        {
            unidadBase.FactorConversion = 1;
        }

        return conversiones;
    }
}
