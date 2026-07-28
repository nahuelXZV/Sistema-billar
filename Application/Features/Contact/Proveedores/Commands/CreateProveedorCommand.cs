using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Contact;
using Domain.DTOs.Purchases;
using Domain.Entities.Contact;
using Domain.Entities.Inventory;
using Domain.Entities.Purchases;
using Infraestructure.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Contact.Proveedores.Commands;

public class CreateProveedorCommand : ICommand<Response<long>>
{
    public required ProveedorDTO ProveedorDTO { get; set; }
}

public class CreateProveedorCommandHandler : ICommandHandler<CreateProveedorCommand, Response<long>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Proveedor> _proveedorRepository;
    private readonly IRepository<ProveedorProducto> _proveedorProductoRepository;
    private readonly IRepository<Producto> _productoRepository;
    private readonly IRepository<ProductoConversion> _productoConversionRepository;
    private readonly IValidator<ProveedorDTO> _proveedorValidator;

    public CreateProveedorCommandHandler(
        IMapper mapper,
        IRepository<Proveedor> proveedorRepository,
        IRepository<ProveedorProducto> proveedorProductoRepository,
        IRepository<Producto> productoRepository,
        IRepository<ProductoConversion> productoConversionRepository,
        IValidator<ProveedorDTO> proveedorValidator)
    {
        _mapper = mapper;
        _proveedorRepository = proveedorRepository;
        _proveedorProductoRepository = proveedorProductoRepository;
        _productoRepository = productoRepository;
        _productoConversionRepository = productoConversionRepository;
        _proveedorValidator = proveedorValidator;
    }

    public async Task<Response<long>> Handle(CreateProveedorCommand request, CancellationToken cancellationToken)
    {
        await _proveedorValidator.ValidateAndThrowAsync(request.ProveedorDTO, cancellationToken);

        var costos = NormalizarCostos(request.ProveedorDTO.ListaProductos);
        await ValidarProductosAsync(costos, cancellationToken);

        var proveedor = _mapper.Map<Proveedor>(request.ProveedorDTO);
        proveedor.Id = 0;
        proveedor.ListaProductos = [];
        proveedor.FechaCreacion = DateTime.Now;
        proveedor.FechaActualizacion = null;

        await _proveedorRepository.AddAsync(proveedor);
        await _proveedorRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        if (costos.Count > 0)
        {
            var productosProveedor = costos.Select(costo =>
            {
                var productoProveedor = _mapper.Map<ProveedorProducto>(costo);
                productoProveedor.Id = 0;
                productoProveedor.IdProveedor = proveedor.Id;
                productoProveedor.FechaActualizacion = DateTime.Now;
                return productoProveedor;
            });

            await _proveedorProductoRepository.AddRangeAsync(productosProveedor);
            await _proveedorProductoRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        }

        return new Response<long>(proveedor.Id);
    }

    private async Task ValidarProductosAsync(IReadOnlyCollection<ProveedorProductoDTO> costos, CancellationToken cancellationToken)
    {
        var idsProductos = costos.Select(costo => costo.IdProducto).Distinct().ToList();
        var productosValidos = await _productoRepository.Query()
            .Where(producto =>
                idsProductos.Contains(producto.Id) &&
                !producto.Eliminado &&
                producto.Activo)
            .Select(producto => producto.Id)
            .ToListAsync(cancellationToken);

        if (productosValidos.Count != idsProductos.Count)
        {
            throw new InvalidOperationException("Uno o más productos seleccionados no existen o están inactivos.");
        }

        var idsConversiones = costos
            .Where(costo => costo.IdProductoConversion.HasValue)
            .Select(costo => costo.IdProductoConversion!.Value)
            .Distinct()
            .ToList();

        if (idsConversiones.Count == 0)
        {
            return;
        }

        var conversiones = await _productoConversionRepository.Query()
            .Where(conversion =>
                idsConversiones.Contains(conversion.Id) &&
                !conversion.Eliminado)
            .ToListAsync(cancellationToken);

        if (conversiones.Count != idsConversiones.Count || costos.Any(costo =>
                costo.IdProductoConversion.HasValue &&
                !conversiones.Any(conversion =>
                    conversion.Id == costo.IdProductoConversion.Value &&
                    conversion.IdProducto == costo.IdProducto)))
        {
            throw new InvalidOperationException("Una o más conversiones no corresponden al producto seleccionado.");
        }
    }

    private static List<ProveedorProductoDTO> NormalizarCostos(IEnumerable<ProveedorProductoDTO>? costos)
    {
        var listaCostos = costos?.ToList() ?? [];

        if (listaCostos.Any(costo => costo.IdProducto <= 0 || costo.CostoReferencial <= 0))
        {
            throw new InvalidOperationException("Cada costo debe tener un producto y un valor mayor a cero.");
        }

        if (listaCostos.Any(costo => costo.IdProductoConversion.HasValue && costo.IdProductoConversion <= 0))
        {
            throw new InvalidOperationException("La conversión seleccionada no es válida.");
        }

        if (listaCostos
            .GroupBy(costo => new { costo.IdProducto, costo.IdProductoConversion })
            .Any(grupo => grupo.Count() > 1))
        {
            throw new InvalidOperationException("No se puede repetir un producto con la misma presentación.");
        }

        return listaCostos;
    }
}
