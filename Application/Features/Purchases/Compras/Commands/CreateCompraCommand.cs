using Application.Features.Inventory.TransaccionInventarios.Command;
using Application.Helpers;
using Application.Interfaces;
using Domain.Common;
using Domain.Constants;
using Domain.DTOs.Inventory;
using Domain.DTOs.Purchases;
using Domain.Entities.Contact;
using Domain.Entities.Inventory;
using Domain.Entities.Purchases;
using Domain.Entities.Security;
using FluentValidation;
using Infraestructure.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static Domain.Constants.Constantes;

namespace Application.Features.Purchases.Compras.Commands;

public class CreateCompraCommand : ICommand<Response<long>>
{
    public required CompraDTO CompraDTO { get; set; }
}

public class CreateCompraCommandHandler : ICommandHandler<CreateCompraCommand, Response<long>>
{
    private readonly IMediator _mediator;
    private readonly IValidator<CompraDTO> _validator;
    private readonly IRepository<Compra> _compraRepository;
    private readonly IRepository<Proveedor> _proveedorRepository;
    private readonly IRepository<Almacen> _almacenRepository;
    private readonly IRepository<Usuario> _usuarioRepository;
    private readonly IRepository<Producto> _productoRepository;
    private readonly IRepository<ProductoConversion> _conversionRepository;
    private readonly IRepository<UnidadMedida> _unidadMedidaRepository;
    private readonly IRepository<Lote> _loteRepository;
    private readonly IRepository<ProveedorProducto> _proveedorProductoRepository;

    public CreateCompraCommandHandler(
        IMediator mediator,
        IValidator<CompraDTO> validator,
        IRepository<Compra> compraRepository,
        IRepository<Proveedor> proveedorRepository,
        IRepository<Almacen> almacenRepository,
        IRepository<Usuario> usuarioRepository,
        IRepository<Producto> productoRepository,
        IRepository<ProductoConversion> conversionRepository,
        IRepository<UnidadMedida> unidadMedidaRepository,
        IRepository<Lote> loteRepository,
        IRepository<ProveedorProducto> proveedorProductoRepository)
    {
        _mediator = mediator;
        _validator = validator;
        _compraRepository = compraRepository;
        _proveedorRepository = proveedorRepository;
        _almacenRepository = almacenRepository;
        _usuarioRepository = usuarioRepository;
        _productoRepository = productoRepository;
        _conversionRepository = conversionRepository;
        _unidadMedidaRepository = unidadMedidaRepository;
        _loteRepository = loteRepository;
        _proveedorProductoRepository = proveedorProductoRepository;
    }

    public async Task<Response<long>> Handle(CreateCompraCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request.CompraDTO, cancellationToken);

        var claveIdempotencia = request.CompraDTO.IdempotencyKey!.Value;
        var idCompraExistente = await _compraRepository.Query()
            .Where(compra => compra.IdempotencyKey == claveIdempotencia)
            .Select(compra => compra.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (idCompraExistente > 0)
        {
            return new Response<long>(idCompraExistente);
        }

        await ValidarCabeceraAsync(request.CompraDTO, cancellationToken);
        var detalles = await CrearDetallesAsync(request.CompraDTO, cancellationToken);

        var compra = new Compra
        {
            IdempotencyKey = claveIdempotencia,
            Numero = string.Empty,
            IdProveedor = request.CompraDTO.IdProveedor,
            IdAlmacen = request.CompraDTO.IdAlmacen,
            IdUsuario = request.CompraDTO.IdUsuario,
            Fecha = DateTime.Now,
            Estado = (short)EstadoCompra.Registrada,
            SubTotal = RedondearMoneda(detalles.Sum(detalle => detalle.SubTotal)),
            Descuento = RedondearMoneda(detalles.Sum(detalle => detalle.Descuento)),
            Total = RedondearMoneda(detalles.Sum(detalle => detalle.Total)),
            Observacion = request.CompraDTO.Observacion?.Trim() ?? string.Empty,
            ListaDetalles = detalles
        };

        foreach (var detalle in compra.ListaDetalles.GroupBy(detalle => new { detalle.IdProducto, detalle.IdProductoConversion }).Select(grupo => grupo.Last()))
        {
            detalle.Compra = compra;
        }

        await _compraRepository.AddAsync(compra);
        await _compraRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        //compra.Numero = $"C-{compra.Fecha:yyyyMMdd}-{compra.Id:D8}";
        compra.Numero = GenerarCodigoHelper.Generar("C", compra.Id);

        _compraRepository.Update(compra);
        await _compraRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        var movimiento = await _mediator.Send(new CreateTransaccionInventarioCommand
        {
            TransaccionInventarioDTO = new TransaccionInventarioDTO
            {
                Tipo = (short)TipoTransaccionInventario.Ingreso,
                Fecha = compra.Fecha,
                Glosa = $"Ingreso por compra {compra.Numero}",
                IdUsuario = compra.IdUsuario,
                IdTransaccionInicial = compra.Id,
                Detalles = compra.ListaDetalles.Select(detalle => new TransaccionInventarioDetalleDTO
                {
                    IdProducto = detalle.IdProducto,
                    IdAlmacen = compra.IdAlmacen,
                    IdLote = detalle.IdLote,
                    Cantidad = (double)detalle.CantidadBase
                }).ToList()
            }
        }, cancellationToken);

        compra.IdTransaccionInventario = movimiento.Data;
        _compraRepository.Update(compra);

        await ActualizarCostosProveedorAsync(compra, cancellationToken);
        await _compraRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<long>(compra.Id);
    }

    private async Task ValidarCabeceraAsync(CompraDTO compra, CancellationToken cancellationToken)
    {
        var proveedorValido = await _proveedorRepository.Query()
            .AnyAsync(proveedor =>
                proveedor.Id == compra.IdProveedor &&
                !proveedor.Eliminado &&
                proveedor.Activo,
                cancellationToken);

        if (!proveedorValido)
        {
            throw new InvalidOperationException("El proveedor seleccionado no existe o está inactivo.");
        }

        var almacenValido = await _almacenRepository.Query()
            .AnyAsync(almacen => almacen.Id == compra.IdAlmacen && !almacen.Eliminado, cancellationToken);

        if (!almacenValido)
        {
            throw new InvalidOperationException("El almacén seleccionado no existe.");
        }

        var usuarioValido = await _usuarioRepository.Query()
            .AnyAsync(usuario =>
                usuario.Id == compra.IdUsuario &&
                !usuario.Eliminado &&
                usuario.Activo,
                cancellationToken);

        if (!usuarioValido)
        {
            throw new InvalidOperationException("El usuario que registra la compra no existe o está inactivo.");
        }
    }

    private async Task<List<CompraDetalle>> CrearDetallesAsync(CompraDTO compra, CancellationToken cancellationToken)
    {
        var detallesSolicitados = compra.ListaDetalles;

        if (detallesSolicitados
            .GroupBy(detalle => new { detalle.IdProducto, detalle.IdProductoConversion, detalle.IdLote })
            .Any(grupo => grupo.Count() > 1))
        {
            throw new InvalidOperationException("No se puede repetir un producto con la misma presentación y lote.");
        }

        var idsProductos = detallesSolicitados.Select(detalle => detalle.IdProducto).Distinct().ToList();
        var productos = await _productoRepository.Query()
            .Where(producto =>
                idsProductos.Contains(producto.Id) &&
                !producto.Eliminado &&
                producto.Activo &&
                producto.Tipo == (short)TipoProducto.Producto)
            .ToListAsync(cancellationToken);

        if (productos.Count != idsProductos.Count)
        {
            throw new InvalidOperationException("Uno o más productos no existen, están inactivos o no afectan inventario.");
        }

        var idsConversiones = detallesSolicitados
            .Where(detalle => detalle.IdProductoConversion.HasValue)
            .Select(detalle => detalle.IdProductoConversion!.Value)
            .Distinct()
            .ToList();

        var conversiones = idsConversiones.Count == 0
            ? []
            : await _conversionRepository.Query()
                .Include(conversion => conversion.UnidadMedida)
                .Where(conversion =>
                    idsConversiones.Contains(conversion.Id) &&
                    !conversion.Eliminado)
                .ToListAsync(cancellationToken);

        if (conversiones.Count != idsConversiones.Count)
        {
            throw new InvalidOperationException("Una o más presentaciones seleccionadas no existen.");
        }

        var idsUnidadesBase = productos.Select(producto => producto.IdUnidadMedida).Distinct().ToList();
        var unidadesBase = await _unidadMedidaRepository.Query()
            .Where(unidad => idsUnidadesBase.Contains(unidad.Id) && !unidad.Eliminado)
            .ToListAsync(cancellationToken);

        if (unidadesBase.Count != idsUnidadesBase.Count)
        {
            throw new InvalidOperationException("Uno o más productos no tienen una unidad de medida base válida.");
        }

        var idsLotes = detallesSolicitados
            .Where(detalle => detalle.IdLote.HasValue)
            .Select(detalle => detalle.IdLote!.Value)
            .Distinct()
            .ToList();

        var lotes = idsLotes.Count == 0
            ? []
            : await _loteRepository.Query()
                .Where(lote =>
                    idsLotes.Contains(lote.Id) &&
                    !lote.Eliminado &&
                    lote.Activo)
                .ToListAsync(cancellationToken);

        if (lotes.Count != idsLotes.Count)
        {
            throw new InvalidOperationException("Uno o más lotes seleccionados no existen o están inactivos.");
        }

        var detalles = new List<CompraDetalle>();

        foreach (var detalleSolicitado in detallesSolicitados)
        {
            var producto = productos.First(item => item.Id == detalleSolicitado.IdProducto);
            var conversion = detalleSolicitado.IdProductoConversion.HasValue
                ? conversiones.FirstOrDefault(item => item.Id == detalleSolicitado.IdProductoConversion.Value)
                : null;

            if (conversion != null && conversion.IdProducto != producto.Id)
            {
                throw new InvalidOperationException($"La presentación no corresponde al producto {producto.Nombre}.");
            }

            if (conversion != null && conversion.FactorConversion <= 0)
            {
                throw new InvalidOperationException($"La presentación de {producto.Nombre} tiene un factor de conversión inválido.");
            }

            var lote = detalleSolicitado.IdLote.HasValue
                ? lotes.First(item => item.Id == detalleSolicitado.IdLote.Value)
                : null;

            if (lote != null && lote.IdProducto != producto.Id)
            {
                throw new InvalidOperationException($"El lote seleccionado no corresponde al producto {producto.Nombre}.");
            }

            var unidad = conversion?.UnidadMedida
                ?? unidadesBase.First(item => item.Id == producto.IdUnidadMedida);
            var factorConversion = conversion?.FactorConversion ?? 1;
            var subtotal = RedondearMoneda(detalleSolicitado.Cantidad * detalleSolicitado.CostoUnitario);

            if (detalleSolicitado.Descuento > subtotal)
            {
                throw new InvalidOperationException($"El descuento de {producto.Nombre} no puede superar su subtotal.");
            }

            detalles.Add(new CompraDetalle
            {
                IdProducto = producto.Id,
                IdProductoConversion = conversion?.Id,
                IdLote = lote?.Id,
                NombreProducto = producto.Nombre,
                NombreUnidadMedida = unidad.Nombre,
                FactorConversion = factorConversion,
                Cantidad = detalleSolicitado.Cantidad,
                CantidadBase = Redondear(detalleSolicitado.Cantidad * factorConversion, 4),
                CostoUnitario = Redondear(detalleSolicitado.CostoUnitario, 6),
                CostoUnitarioBase = Redondear(detalleSolicitado.CostoUnitario / factorConversion, 6),
                Descuento = RedondearMoneda(detalleSolicitado.Descuento),
                SubTotal = subtotal,
                Total = RedondearMoneda(subtotal - detalleSolicitado.Descuento)
            });
        }

        return detalles;
    }

    private async Task ActualizarCostosProveedorAsync(Compra compra, CancellationToken cancellationToken)
    {
        var costosActuales = await _proveedorProductoRepository.Query()
            .Where(costo =>
                costo.IdProveedor == compra.IdProveedor &&
                !costo.Eliminado)
            .ToListAsync(cancellationToken);

        foreach (var detalle in compra.ListaDetalles)
        {
            var costoActual = costosActuales.FirstOrDefault(costo =>
                costo.IdProducto == detalle.IdProducto &&
                costo.IdProductoConversion == detalle.IdProductoConversion);

            if (costoActual == null)
            {
                await _proveedorProductoRepository.AddAsync(new ProveedorProducto
                {
                    IdProveedor = compra.IdProveedor,
                    IdProducto = detalle.IdProducto,
                    IdProductoConversion = detalle.IdProductoConversion,
                    CostoReferencial = detalle.CostoUnitario,
                    FechaActualizacion = DateTime.Now
                });
                continue;
            }

            _proveedorProductoRepository.Attach(costoActual);
            costoActual.CostoReferencial = detalle.CostoUnitario;
            costoActual.FechaActualizacion = DateTime.Now;
        }
    }

    private static decimal RedondearMoneda(decimal valor) => Redondear(valor, 2);

    private static decimal Redondear(decimal valor, int decimales) =>
        decimal.Round(valor, decimales, MidpointRounding.AwayFromZero);
}
