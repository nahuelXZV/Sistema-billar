using Application.Interfaces;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Inventory;
using Domain.Entities.Sales;
using Domain.Utils;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sales.Ventas.Commands;

public class ValidarVentaCommand : ICommand<Response<bool>>
{
    public required VentaDTO Venta { get; set; }
}

public class ValidarVentaCommandHandler : ICommandHandler<ValidarVentaCommand, Response<bool>>
{
    private readonly IRepository<Producto> _productoRepository;

    public ValidarVentaCommandHandler(IRepository<Producto> productoRepository)
    {
        _productoRepository = productoRepository;
    }

    public async Task<Response<bool>> Handle(ValidarVentaCommand solicitud, CancellationToken tokenCancelacion)
    {
        await NormalizarProductosYPreciosAsync(solicitud.Venta, tokenCancelacion);
        ValidarImportes(solicitud.Venta);
        await ValidarFinalizacionOrdenAsync(solicitud.Venta, tokenCancelacion);
        return new Response<bool>(true);
    }

    private static void ValidarImportes(VentaDTO venta)
    {
        var detalles = venta.ListaDetalles ?? [];
        var pagos = venta.ListaPagos ?? [];

        if (detalles.Count == 0)
        {
            throw new InvalidOperationException("La venta debe contener al menos un detalle.");
        }

        if (venta.Descuento < 0 || venta.Recargo < 0)
        {
            throw new InvalidOperationException("El descuento y el recargo no pueden ser negativos.");
        }

        decimal subtotalCalculado = 0;
        decimal totalDetallesCalculado = 0;

        foreach (var detalle in detalles)
        {
            if (detalle.IdProducto <= 0)
            {
                throw new InvalidOperationException("Todos los detalles deben tener un producto válido.");
            }

            if (detalle.Cantidad <= 0 || detalle.PrecioUnitario < 0)
            {
                throw new InvalidOperationException($"La cantidad y el precio del producto {detalle.IdProducto} no son válidos.");
            }

            var descuentoDetalle = detalle.Descuento ?? 0;
            if (descuentoDetalle < 0)
            {
                throw new InvalidOperationException($"El descuento del producto {detalle.IdProducto} no puede ser negativo.");
            }

            var subtotalDetalle = Utils.Redondear(detalle.Cantidad * detalle.PrecioUnitario);
            if (descuentoDetalle > subtotalDetalle)
            {
                throw new InvalidOperationException($"El descuento del producto {detalle.IdProducto} no puede superar su subtotal.");
            }

            var totalDetalle = Utils.Redondear(subtotalDetalle - descuentoDetalle);
            if (!Coincide(detalle.SubTotal, subtotalDetalle) || !Coincide(detalle.Total, totalDetalle))
            {
                throw new InvalidOperationException($"Los importes del producto {detalle.IdProducto} no coinciden con su cantidad, precio y descuento.");
            }

            subtotalCalculado += subtotalDetalle;
            totalDetallesCalculado += totalDetalle;
        }

        subtotalCalculado = Utils.Redondear(subtotalCalculado);
        totalDetallesCalculado = Utils.Redondear(totalDetallesCalculado);

        if (venta.Descuento > totalDetallesCalculado)
        {
            throw new InvalidOperationException("El descuento de la venta no puede superar el total de sus detalles.");
        }

        var totalCalculado = Utils.Redondear(totalDetallesCalculado - venta.Descuento + venta.Recargo);

        if (totalCalculado <= 0)
        {
            throw new InvalidOperationException("El total de la venta debe ser mayor a cero.");
        }

        if (!Coincide(venta.SubTotal, subtotalCalculado) || !Coincide(venta.Total, totalCalculado))
        {
            throw new InvalidOperationException("El subtotal o el total de la venta no coincide con sus detalles, descuento y recargo.");
        }

        if (pagos.Count == 0)
        {
            throw new InvalidOperationException("La venta debe contener al menos un pago.");
        }

        foreach (var pago in pagos)
        {
            if (pago.IdMetodoPago <= 0 || pago.MontoTotal <= 0)
            {
                throw new InvalidOperationException("Todos los pagos deben tener un método y un monto mayor a cero.");
            }
        }

        var totalPagosCalculado = Utils.Redondear(pagos.Sum(pago => pago.MontoTotal));
        if (!Coincide(venta.TotalPagado, totalPagosCalculado))
        {
            throw new InvalidOperationException("El total pagado no coincide con la suma de los pagos.");
        }

        if (totalPagosCalculado < totalCalculado)
        {
            throw new InvalidOperationException("El total pagado no cubre el total de la venta.");
        }

        var cambioCalculado = Utils.Redondear(totalPagosCalculado - totalCalculado);
        if (!Coincide(venta.Cambio, cambioCalculado))
        {
            throw new InvalidOperationException("El cambio no coincide con el total pagado y el total de la venta.");
        }
    }

    private async Task NormalizarProductosYPreciosAsync(VentaDTO venta, CancellationToken tokenCancelacion)
    {
        var detalles = venta.ListaDetalles ?? [];
        var idsProductos = detalles.Select(detalle => detalle.IdProducto).Distinct().ToList();

        var productosValidos = await _productoRepository.Query()
            .Where(producto => idsProductos.Contains(producto.Id) && producto.Activo && !producto.Eliminado)
            .ToListAsync(tokenCancelacion);

        var idsProductosInvalidos = idsProductos.Except(productosValidos.Select(producto => producto.Id)).ToList();
        if (idsProductosInvalidos.Count > 0)
        {
            throw new InvalidOperationException(
                $"Los siguientes productos no existen o están inactivos: {string.Join(", ", idsProductosInvalidos)}.");
        }

        var idsDetallesOrden = detalles
            .Where(detalle => detalle.IdOrdenVentaDetalle.HasValue)
            .Select(detalle => detalle.IdOrdenVentaDetalle!.Value)
            .Distinct()
            .ToList();

        var detallesOrden = idsDetallesOrden.Count == 0
            ? []
            : await _productoRepository.Query<OrdenVentaDetalle>()
                .Where(detalle => !detalle.Eliminado && idsDetallesOrden.Contains(detalle.Id))
                .ToListAsync(tokenCancelacion);

        var idsConversiones = detalles
            .Where(detalle => detalle.IdProductoConversion.HasValue)
            .Select(detalle => detalle.IdProductoConversion!.Value)
            .Distinct()
            .ToList();

        var conversiones = idsConversiones.Count == 0
            ? []
            : await _productoRepository.Query<ProductoConversion>()
                .Include(conversion => conversion.UnidadMedida)
                .Where(conversion => !conversion.Eliminado && idsConversiones.Contains(conversion.Id))
                .ToListAsync(tokenCancelacion);

        var detallesDirectos = detalles.Where(detalle => !detalle.IdOrdenVentaDetalle.HasValue).ToList();
        var idListaPrecio = detallesDirectos.Count == 0
            ? 0
            : await _productoRepository.Query<Vendedor>()
                .Where(vendedor =>
                    !vendedor.Eliminado &&
                    vendedor.Activo &&
                    vendedor.Id == venta.IdVendedor)
                .Select(vendedor => vendedor.IdListaPrecio)
                .FirstOrDefaultAsync(tokenCancelacion);

        if (detallesDirectos.Count > 0 && idListaPrecio <= 0)
        {
            throw new InvalidOperationException("El vendedor no tiene una lista de precios asignada.");
        }

        var idsConversionesDirectas = detallesDirectos
            .Where(detalle => detalle.IdProductoConversion.HasValue)
            .Select(detalle => detalle.IdProductoConversion!.Value)
            .Distinct()
            .ToList();

        var precios = idsConversionesDirectas.Count == 0
            ? []
            : await _productoRepository.Query<ListaPreciosDetalle>()
                .Where(detalle =>
                    !detalle.Eliminado &&
                    detalle.IdListaPrecio == idListaPrecio &&
                    idsConversionesDirectas.Contains(detalle.IdProductoConversion))
                .ToListAsync(tokenCancelacion);

        foreach (var detalle in detalles)
        {
            var producto = productosValidos.First(item => item.Id == detalle.IdProducto);
            detalle.NombreProducto = producto.Nombre;

            if (detalle.IdOrdenVentaDetalle.HasValue)
            {
                NormalizarDesdeOrden(venta, detalle, producto, detallesOrden);
                continue;
            }

            NormalizarVentaDirecta(detalle, producto, conversiones, precios);
        }
    }

    private async Task ValidarFinalizacionOrdenAsync(VentaDTO venta, CancellationToken tokenCancelacion)
    {
        if (!venta.FinalizarOrdenVenta)
        {
            return;
        }

        if (!venta.IdOrdenVenta.HasValue || venta.IdOrdenVenta.Value <= 0)
        {
            throw new InvalidOperationException("Solo se puede finalizar una orden de venta existente.");
        }

        var detallesOrden = await _productoRepository.Query<OrdenVentaDetalle>()
            .Where(detalle => !detalle.Eliminado && detalle.IdOrdenVenta == venta.IdOrdenVenta.Value)
            .ToListAsync(tokenCancelacion);

        if (detallesOrden.Count == 0)
        {
            throw new InvalidOperationException("La orden no tiene detalles pendientes para finalizar.");
        }

        var detallesPagados = venta.ListaDetalles ?? [];
        if (detallesPagados.Any(detalle => !detalle.IdOrdenVentaDetalle.HasValue))
        {
            throw new InvalidOperationException("Para finalizar la orden, todos los detalles pagados deben pertenecer a la orden.");
        }

        foreach (var detalleOrden in detallesOrden)
        {
            var cantidadPagada = detallesPagados
                .Where(detalle => detalle.IdOrdenVentaDetalle == detalleOrden.Id)
                .Sum(detalle => detalle.Cantidad);

            if (Utils.Redondear(cantidadPagada) != Utils.Redondear(detalleOrden.Cantidad))
            {
                throw new InvalidOperationException("Para finalizar la orden, todos sus detalles deben pagarse por completo.");
            }
        }
    }

    private static void NormalizarDesdeOrden(
        VentaDTO venta,
        VentaDetalleDTO detalle,
        Producto producto,
        IReadOnlyCollection<OrdenVentaDetalle> detallesOrden)
    {
        var detalleOrden = detallesOrden.FirstOrDefault(item => item.Id == detalle.IdOrdenVentaDetalle)
            ?? throw new InvalidOperationException($"El detalle de orden {detalle.IdOrdenVentaDetalle} no existe.");

        if (venta.IdOrdenVenta != detalleOrden.IdOrdenVenta ||
            detalleOrden.IdProducto != detalle.IdProducto ||
            detalleOrden.IdProductoConversion != detalle.IdProductoConversion)
        {
            throw new InvalidOperationException($"El detalle pagado de {producto.Nombre} no coincide con la orden.");
        }

        detalle.PrecioUnitario = detalleOrden.PrecioUnitario;
        detalle.NombreUnidadMedida = detalleOrden.NombreUnidadMedida;
        detalle.AbreviaturaUnidadMedida = detalleOrden.AbreviaturaUnidadMedida;
        detalle.FactorConversion = detalleOrden.FactorConversion > 0 ? detalleOrden.FactorConversion : 1;
    }

    private static void NormalizarVentaDirecta(
        VentaDetalleDTO detalle,
        Producto producto,
        IReadOnlyCollection<ProductoConversion> conversiones,
        IReadOnlyCollection<ListaPreciosDetalle> precios)
    {
        if (!detalle.IdProductoConversion.HasValue)
        {
            throw new InvalidOperationException($"Debe seleccionar una unidad de medida para {producto.Nombre}.");
        }

        var conversion = conversiones.FirstOrDefault(item => item.Id == detalle.IdProductoConversion.Value)
            ?? throw new InvalidOperationException($"La unidad seleccionada para {producto.Nombre} no existe.");

        if (conversion.IdProducto != producto.Id || conversion.FactorConversion <= 0)
        {
            throw new InvalidOperationException($"La unidad seleccionada no corresponde a {producto.Nombre}.");
        }

        var precio = precios.FirstOrDefault(item => item.IdProductoConversion == conversion.Id)
            ?? throw new InvalidOperationException(
                $"La presentación seleccionada de {producto.Nombre} no tiene precio para este vendedor.");

        if (precio.Precio <= 0)
        {
            throw new InvalidOperationException($"El precio de {producto.Nombre} debe ser mayor a cero.");
        }

        detalle.PrecioUnitario = precio.Precio;
        detalle.NombreUnidadMedida = conversion.UnidadMedida?.Nombre ?? string.Empty;
        detalle.AbreviaturaUnidadMedida = conversion.UnidadMedida?.Abreviatura ?? string.Empty;
        detalle.FactorConversion = conversion.FactorConversion;
    }

    private static bool Coincide(decimal valorRecibido, decimal valorCalculado) => Utils.Redondear(valorRecibido) == Utils.Redondear(valorCalculado);
}
