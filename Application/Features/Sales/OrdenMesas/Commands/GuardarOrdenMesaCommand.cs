using Application.Common.Utils;
using Application.Features.Sales.OrdenVentas.Commands;
using Application.Interfaces;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Contact;
using Domain.Entities.Inventory;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sales.OrdenMesas.Commands;

public class GuardarOrdenMesaCommand : ICommand<Response<OrdenMesaDTO>>
{
    public required OrdenMesaDTO OrdenMesa { get; set; }
}

public class GuardarOrdenMesaCommandHandler : ICommandHandler<GuardarOrdenMesaCommand, Response<OrdenMesaDTO>>
{
    private readonly IMediator _mediator;
    private readonly IRepository<OrdenVenta> _ordenRepository;
    private readonly IRepository<UsoMesa> _usoMesaRepository;
    private readonly IRepository<Producto> _productoRepository;
    private readonly IRepository<ProductoConversion> _conversionRepository;
    private readonly IRepository<ListaPreciosDetalle> _listaPrecioDetalleRepository;
    private readonly IRepository<OrdenVentaDetalle> _ordenDetalleRepository;

    public GuardarOrdenMesaCommandHandler(
        IMediator mediator,
        IRepository<OrdenVenta> ordenRepository,
        IRepository<UsoMesa> usoMesaRepository,
        IRepository<Producto> productoRepository,
        IRepository<ProductoConversion> conversionRepository,
        IRepository<ListaPreciosDetalle> listaPrecioDetalleRepository,
        IRepository<OrdenVentaDetalle> ordenDetalleRepository)
    {
        _mediator = mediator;
        _ordenRepository = ordenRepository;
        _usoMesaRepository = usoMesaRepository;
        _productoRepository = productoRepository;
        _conversionRepository = conversionRepository;
        _listaPrecioDetalleRepository = listaPrecioDetalleRepository;
        _ordenDetalleRepository = ordenDetalleRepository;
    }

    public async Task<Response<OrdenMesaDTO>> Handle(GuardarOrdenMesaCommand request, CancellationToken tokenCancelacion)
    {
        var ordenId = request.OrdenMesa.Id;

        ValidarSolicitud(request.OrdenMesa);
        await ValidarProductos(request.OrdenMesa.Detalles, tokenCancelacion);
        await ValidarClientes(request.OrdenMesa, tokenCancelacion);
        await NormalizarPresentacionesYPrecios(request.OrdenMesa, tokenCancelacion);

        if (ordenId == 0)
        {
            ordenId = (await _mediator.Send(new CreateOrdenVentaCommand() { OrdenMesa = request.OrdenMesa })).Data;
        }
        else
        {
            var respuesta = await _mediator.Send(new UpdateOrdenVentaCommand() { OrdenMesa = request.OrdenMesa }, tokenCancelacion);
        }

        var orden = await _ordenRepository.Query()
                    .Include(o => o.ListaDetalles.Where(detalle => !detalle.Eliminado))
                    .FirstOrDefaultAsync(ordenActual => !ordenActual.Eliminado && ordenActual.Id == ordenId, tokenCancelacion)
                    ?? throw new InvalidOperationException("Error al crear la orden de mesa.");

        var usoMesa = await _usoMesaRepository.Query().FirstOrDefaultAsync(uso => !uso.Eliminado && uso.IdOrdenVenta == orden.Id && uso.IdMesa == request.OrdenMesa.IdMesa,
                tokenCancelacion) ?? throw new InvalidOperationException("Error al crear el uso de la mesa.");

        var ordenMesaResponse = OrdenMesaUtils.Mapear(orden, usoMesa, orden.ListaDetalles);
        return new Response<OrdenMesaDTO>(ordenMesaResponse);
    }

    #region Validators
    private static void ValidarSolicitud(OrdenMesaDTO ordenMesa)
    {
        if (ordenMesa.IdMesa <= 0) throw new InvalidOperationException("Debe seleccionar una mesa.");

        if (ordenMesa.IdVendedor <= 0) throw new InvalidOperationException("Debe existir un vendedor para guardar la orden.");

        if (ordenMesa.DescuentoGlobal < 0 || ordenMesa.RecargoGlobal < 0) throw new InvalidOperationException("El descuento y el recargo no pueden ser negativos.");

        foreach (var detalle in ordenMesa.Detalles)
        {
            if (detalle.IdProducto <= 0 || detalle.Cantidad <= 0 || detalle.PrecioUnitario < 0)
                throw new InvalidOperationException("Los detalles de la orden contienen valores inválidos.");

            if (detalle.Descuento < 0)
                throw new InvalidOperationException("El descuento de un detalle no puede ser negativo.");
        }
    }

    private async Task ValidarProductos(IEnumerable<OrdenMesaDetalleDTO> detalles, CancellationToken tokenCancelacion)
    {
        var ids = detalles.Select(detalle => detalle.IdProducto).Distinct().ToList();
        if (ids.Count == 0) return;

        var cantidadProductosValidos = await _productoRepository.Query()
            .CountAsync(producto => ids.Contains(producto.Id) && producto.Activo && !producto.Eliminado, tokenCancelacion);

        if (cantidadProductosValidos != ids.Count) throw new InvalidOperationException("La orden contiene productos inexistentes o inactivos.");
    }

    private async Task ValidarClientes(OrdenMesaDTO ordenMesa, CancellationToken tokenCancelacion)
    {
        foreach (var detalle in ordenMesa.Detalles)
        {
            detalle.IdCliente ??= ordenMesa.IdCliente;
        }

        var idsClientes = ordenMesa.Detalles
            .Select(detalle => detalle.IdCliente)
            .ToList();

        if (idsClientes.Any(idCliente => !idCliente.HasValue || idCliente.Value <= 0))
        {
            throw new InvalidOperationException("Cada detalle de la orden debe tener un cliente asignado.");
        }

        var idsClientesDistintos = idsClientes
            .Select(idCliente => idCliente!.Value)
            .Distinct()
            .ToList();

        var cantidadClientesValidos = await _productoRepository.Query<Cliente>()
            .CountAsync(cliente => idsClientesDistintos.Contains(cliente.Id) && !cliente.Eliminado, tokenCancelacion);

        if (cantidadClientesValidos != idsClientesDistintos.Count)
        {
            throw new InvalidOperationException("La orden contiene clientes inexistentes o eliminados.");
        }
    }

    private async Task NormalizarPresentacionesYPrecios(OrdenMesaDTO ordenMesa, CancellationToken tokenCancelacion)
    {
        if (ordenMesa.Detalles.Count == 0)
        {
            return;
        }

        var idListaPrecio = await _productoRepository.Query<Vendedor>()
            .Where(vendedor =>
                !vendedor.Eliminado &&
                vendedor.Activo &&
                vendedor.Id == ordenMesa.IdVendedor)
            .Select(vendedor => vendedor.IdListaPrecio)
            .FirstOrDefaultAsync(tokenCancelacion);

        if (idListaPrecio <= 0)
        {
            throw new InvalidOperationException("El vendedor no tiene una lista de precios asignada.");
        }

        var idsProductos = ordenMesa.Detalles
            .Select(detalle => detalle.IdProducto)
            .Distinct()
            .ToList();

        var productos = await _productoRepository.Query()
            .Where(producto => idsProductos.Contains(producto.Id) && producto.Activo && !producto.Eliminado)
            .ToListAsync(tokenCancelacion);

        var idsDetallesExistentes = ordenMesa.Detalles
            .Where(detalle => detalle.Id > 0)
            .Select(detalle => detalle.Id)
            .Distinct()
            .ToList();

        var detallesExistentes = idsDetallesExistentes.Count == 0
            ? []
            : await _ordenDetalleRepository.Query()
                .Where(detalle =>
                    !detalle.Eliminado &&
                    detalle.IdOrdenVenta == ordenMesa.Id &&
                    idsDetallesExistentes.Contains(detalle.Id))
                .ToListAsync(tokenCancelacion);

        var idsConversiones = ordenMesa.Detalles
            .Where(detalle => detalle.IdProductoConversion.HasValue)
            .Select(detalle => detalle.IdProductoConversion!.Value)
            .Distinct()
            .ToList();

        var conversiones = idsConversiones.Count == 0
            ? []
            : await _conversionRepository.Query()
                .Include(conversion => conversion.UnidadMedida)
                .Where(conversion =>
                    !conversion.Eliminado &&
                    idsConversiones.Contains(conversion.Id))
                .ToListAsync(tokenCancelacion);

        var precios = idsConversiones.Count == 0
            ? []
            : await _listaPrecioDetalleRepository.Query()
                .Where(detalle =>
                    !detalle.Eliminado &&
                    detalle.IdListaPrecio == idListaPrecio &&
                    idsConversiones.Contains(detalle.IdProductoConversion))
                .ToListAsync(tokenCancelacion);

        foreach (var detalle in ordenMesa.Detalles)
        {
            detalle.NombreProducto = productos.First(producto => producto.Id == detalle.IdProducto).Nombre;

            var detalleExistente = detalle.Id > 0
                ? detallesExistentes.FirstOrDefault(item => item.Id == detalle.Id)
                : null;

            if (detalleExistente is not null)
            {
                if (detalleExistente.IdProducto != detalle.IdProducto ||
                    detalleExistente.IdProductoConversion != detalle.IdProductoConversion)
                {
                    throw new InvalidOperationException("No se puede cambiar el producto o la unidad de un detalle existente.");
                }

                detalle.PrecioUnitario = detalleExistente.PrecioUnitario;
                detalle.NombreUnidadMedida = detalleExistente.NombreUnidadMedida;
                detalle.AbreviaturaUnidadMedida = detalleExistente.AbreviaturaUnidadMedida;
                detalle.FactorConversion = detalleExistente.FactorConversion;
            }
            else
            {
                if (!detalle.IdProductoConversion.HasValue)
                {
                    throw new InvalidOperationException($"Debe seleccionar una unidad de medida para {detalle.NombreProducto}.");
                }

                var conversion = conversiones.FirstOrDefault(item => item.Id == detalle.IdProductoConversion.Value)
                    ?? throw new InvalidOperationException($"La unidad seleccionada para {detalle.NombreProducto} no existe.");

                if (conversion.IdProducto != detalle.IdProducto || conversion.FactorConversion <= 0)
                {
                    throw new InvalidOperationException($"La unidad seleccionada no corresponde a {detalle.NombreProducto}.");
                }

                var precio = precios.FirstOrDefault(item => item.IdProductoConversion == conversion.Id)
                    ?? throw new InvalidOperationException($"La unidad {conversion.UnidadMedida?.Nombre} no tiene precio en la lista del vendedor.");

                if (precio.Precio <= 0)
                {
                    throw new InvalidOperationException($"El precio de {detalle.NombreProducto} debe ser mayor a cero.");
                }

                detalle.PrecioUnitario = precio.Precio;
                detalle.NombreUnidadMedida = conversion.UnidadMedida?.Nombre ?? string.Empty;
                detalle.AbreviaturaUnidadMedida = conversion.UnidadMedida?.Abreviatura ?? string.Empty;
                detalle.FactorConversion = conversion.FactorConversion;
            }

            var subtotal = Domain.Utils.Utils.Redondear(detalle.Cantidad * detalle.PrecioUnitario);
            if (detalle.Descuento > subtotal)
            {
                throw new InvalidOperationException($"El descuento de {detalle.NombreProducto} supera su subtotal.");
            }

            detalle.SubTotal = subtotal;
            detalle.Total = Domain.Utils.Utils.Redondear(subtotal - detalle.Descuento);
        }

        var duplicados = ordenMesa.Detalles
            .Where(detalle => !detalle.EsTiempoMesa)
            .GroupBy(detalle => new { detalle.IdProducto, detalle.IdProductoConversion, detalle.IdCliente })
            .Any(grupo => grupo.Count() > 1);

        if (duplicados)
        {
            throw new InvalidOperationException("La orden contiene líneas duplicadas para la misma presentación.");
        }
    }
    #endregion
}
