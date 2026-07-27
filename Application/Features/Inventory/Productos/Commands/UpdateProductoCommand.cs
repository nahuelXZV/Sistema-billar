using Application.Features.Inventory.Categorias.Commands;
using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.Productos.Commands;

public class UpdateProductoCommand : ICommand<Response<bool>>
{
    public required ProductoDTO ProductoDTO { get; set; }
}

public class UpdateProductoCommandHandler : ICommandHandler<UpdateProductoCommand, Response<bool>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Producto> _repository;
    private readonly IRepository<ProductoCompuesto> _rpProductoCompuesto;
    private readonly IRepository<ProductoConversion> _rpProductoConversion;

    public UpdateProductoCommandHandler(
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

    public async Task<Response<bool>> Handle(UpdateProductoCommand request, CancellationToken cancellationToken)
    {
        var conversiones = NormalizarConversiones(request.ProductoDTO);
        var producto = await _repository.GetByIdAsync(request.ProductoDTO.Id);
        if (producto == null) throw new Exception("El producto no existe.");

        _repository.Attach(producto);
        _mapper.Map(request.ProductoDTO, producto);

        var productosCompuestosExistentes = await _rpProductoCompuesto.Query()
            .Where(pc => pc.IdProductoPadre == producto.Id && !pc.Eliminado)
            .ToListAsync(cancellationToken);

        if (productosCompuestosExistentes.Count > 0)
            _rpProductoCompuesto.DeleteRange(productosCompuestosExistentes);

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

        var conversionesExistentes = await _rpProductoConversion.Query()
            .Where(conversion => conversion.IdProducto == producto.Id && !conversion.Eliminado)
            .ToListAsync(cancellationToken);

        var idsConversionesSolicitadas = conversiones
            .Where(conversion => conversion.Id > 0)
            .Select(conversion => conversion.Id)
            .ToHashSet();

        if (idsConversionesSolicitadas.Any(id =>
            conversionesExistentes.All(conversion => conversion.Id != id)))
        {
            throw new InvalidOperationException("Una o más conversiones no pertenecen al producto.");
        }

        var conversionesEliminadas = conversionesExistentes
            .Where(conversion => !idsConversionesSolicitadas.Contains(conversion.Id))
            .ToList();

        if (conversionesEliminadas.Count > 0)
        {
            var idsConversionesEliminadas = conversionesEliminadas
                .Select(conversion => conversion.Id)
                .ToList();

            var tienePreciosAsociados = await _rpProductoConversion.Query<ListaPreciosDetalle>()
                .AnyAsync(
                    detalle =>
                        !detalle.Eliminado &&
                        idsConversionesEliminadas.Contains(detalle.IdProductoConversion),
                    cancellationToken);

            if (tienePreciosAsociados)
            {
                throw new InvalidOperationException(
                    "No se puede eliminar una unidad que tiene precios asociados. " +
                    "Elimínala primero de las listas de precios.");
            }

            _rpProductoConversion.DeleteRange(conversionesEliminadas);
        }

        foreach (var conversionDto in conversiones)
        {
            if (conversionDto.Id > 0)
            {
                var conversionExistente = conversionesExistentes.First(
                    conversion => conversion.Id == conversionDto.Id);

                conversionExistente.IdUnidadMedida = conversionDto.IdUnidadMedida;
                conversionExistente.FactorConversion = conversionDto.FactorConversion;
                _rpProductoConversion.Update(conversionExistente);
                continue;
            }

            await _rpProductoConversion.AddAsync(new ProductoConversion
            {
                IdProducto = producto.Id,
                IdUnidadMedida = conversionDto.IdUnidadMedida,
                FactorConversion = conversionDto.FactorConversion
            });
        }

        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
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


