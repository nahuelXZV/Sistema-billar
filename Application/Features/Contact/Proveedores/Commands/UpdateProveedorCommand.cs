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

public class UpdateProveedorCommand : ICommand<Response<bool>>
{
    public required ProveedorDTO ProveedorDTO { get; set; }
}

public class UpdateProveedorCommandHandler : ICommandHandler<UpdateProveedorCommand, Response<bool>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Proveedor> _proveedorRepository;
    private readonly IRepository<ProveedorProducto> _proveedorProductoRepository;
    private readonly IRepository<Producto> _productoRepository;
    private readonly IRepository<ProductoConversion> _productoConversionRepository;
    private readonly IValidator<ProveedorDTO> _proveedorValidator;

    public UpdateProveedorCommandHandler(
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

    public async Task<Response<bool>> Handle(UpdateProveedorCommand request, CancellationToken cancellationToken)
    {
        if (request.ProveedorDTO.Id <= 0)
        {
            throw new ArgumentException("El proveedor seleccionado no es válido.");
        }

        await _proveedorValidator.ValidateAndThrowAsync(request.ProveedorDTO, cancellationToken);

        var proveedor = await _proveedorRepository.Query()
            .FirstOrDefaultAsync(item => item.Id == request.ProveedorDTO.Id && !item.Eliminado, cancellationToken)
            ?? throw new ArgumentException("El proveedor no existe.");

        var costos = NormalizarCostos(request.ProveedorDTO.ListaProductos);
        await ValidarProductosAsync(costos, cancellationToken);

        _proveedorRepository.Attach(proveedor);
        _mapper.Map(request.ProveedorDTO, proveedor);
        proveedor.FechaActualizacion = DateTime.Now;

        var costosActuales = await _proveedorProductoRepository.Query()
            .Where(costo => costo.IdProveedor == proveedor.Id)
            .ToListAsync(cancellationToken);

        var idsCostosSolicitados = costos
            .Where(costo => costo.Id > 0)
            .Select(costo => costo.Id)
            .ToHashSet();

        if (idsCostosSolicitados.Any(id => costosActuales.All(costo => costo.Id != id)))
        {
            throw new InvalidOperationException("Uno o más costos no pertenecen al proveedor.");
        }

        foreach (var costoDto in costos.Where(costo => costo.Id > 0))
        {
            var costoActual = costosActuales.First(costo => costo.Id == costoDto.Id);
            _proveedorProductoRepository.Attach(costoActual);
            _mapper.Map(costoDto, costoActual);
            costoActual.Eliminado = false;
            costoActual.FechaActualizacion = DateTime.Now;
        }

        var nuevosCostos = costos
            .Where(costo => costo.Id == 0)
            .Select(costo =>
            {
                var nuevoCosto = _mapper.Map<ProveedorProducto>(costo);
                nuevoCosto.IdProveedor = proveedor.Id;
                nuevoCosto.FechaActualizacion = DateTime.Now;
                return nuevoCosto;
            })
            .ToList();

        if (nuevosCostos.Count > 0)
        {
            await _proveedorProductoRepository.AddRangeAsync(nuevosCostos);
        }

        var costosEliminados = costosActuales
            .Where(costo => !costo.Eliminado && !idsCostosSolicitados.Contains(costo.Id))
            .ToList();

        if (costosEliminados.Count > 0)
        {
            _proveedorProductoRepository.DeleteRange(costosEliminados);
        }

        await _proveedorRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
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
