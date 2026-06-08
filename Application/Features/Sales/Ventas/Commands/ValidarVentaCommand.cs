using Application.Interfaces;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Inventory;
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
        ValidarImportes(solicitud.Venta);
        await ValidarProductosAsync(solicitud.Venta.ListaDetalles ?? [], tokenCancelacion);

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

            var subtotalDetalle = Redondear(detalle.Cantidad * detalle.PrecioUnitario);
            if (descuentoDetalle > subtotalDetalle)
            {
                throw new InvalidOperationException($"El descuento del producto {detalle.IdProducto} no puede superar su subtotal.");
            }

            var totalDetalle = Redondear(subtotalDetalle - descuentoDetalle);
            if (!Coincide(detalle.SubTotal, subtotalDetalle) || !Coincide(detalle.Total, totalDetalle))
            {
                throw new InvalidOperationException($"Los importes del producto {detalle.IdProducto} no coinciden con su cantidad, precio y descuento.");
            }

            subtotalCalculado += subtotalDetalle;
            totalDetallesCalculado += totalDetalle;
        }

        subtotalCalculado = Redondear(subtotalCalculado);
        totalDetallesCalculado = Redondear(totalDetallesCalculado);

        if (venta.Descuento > totalDetallesCalculado)
        {
            throw new InvalidOperationException("El descuento de la venta no puede superar el total de sus detalles.");
        }

        var totalCalculado = Redondear(totalDetallesCalculado - venta.Descuento + venta.Recargo);

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

        var totalPagosCalculado = Redondear(pagos.Sum(pago => pago.MontoTotal));
        if (!Coincide(venta.TotalPagado, totalPagosCalculado))
        {
            throw new InvalidOperationException("El total pagado no coincide con la suma de los pagos.");
        }

        if (totalPagosCalculado < totalCalculado)
        {
            throw new InvalidOperationException("El total pagado no cubre el total de la venta.");
        }

        var cambioCalculado = Redondear(totalPagosCalculado - totalCalculado);
        if (!Coincide(venta.Cambio, cambioCalculado))
        {
            throw new InvalidOperationException("El cambio no coincide con el total pagado y el total de la venta.");
        }
    }

    private async Task ValidarProductosAsync(IEnumerable<VentaDetalleDTO> detalles, CancellationToken tokenCancelacion)
    {
        var idsProductos = detalles
            .Select(detalle => detalle.IdProducto)
            .Distinct()
            .ToList();

        var productosValidos = await _productoRepository.Query()
            .Where(producto =>
                idsProductos.Contains(producto.Id) &&
                producto.Activo &&
                !producto.Eliminado)
            .Select(producto => producto.Id)
            .ToListAsync(tokenCancelacion);

        var idsProductosInvalidos = idsProductos
            .Except(productosValidos)
            .ToList();

        if (idsProductosInvalidos.Count > 0)
        {
            throw new InvalidOperationException($"Los siguientes productos no existen o están inactivos: {string.Join(", ", idsProductosInvalidos)}.");
        }
    }

    private static decimal Redondear(decimal valor) =>
        Math.Round(valor, 2, MidpointRounding.AwayFromZero);

    private static bool Coincide(decimal valorRecibido, decimal valorCalculado) =>
        Redondear(valorRecibido) == Redondear(valorCalculado);
}
