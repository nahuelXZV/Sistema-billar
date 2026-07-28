using Application.Features.Inventory.TransaccionInventarios.Command;
using Application.Features.Sales.Vendedores.Queries;
using Application.Interfaces;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.DTOs.Sales;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static Domain.Constants.Constantes;

namespace Application.Features.Sales.Ventas.Commands;

public class CrearMovimientoInventarioVentaCommand : ICommand<Response<bool>>
{
    public long IdVenta { get; set; }
    public long IdVendedor { get; set; }
    public IReadOnlyCollection<VentaDetalleDTO> DetallesVenta { get; set; } = [];
}

public class CrearMovimientoInventarioVentaCommandHandler : ICommandHandler<CrearMovimientoInventarioVentaCommand, Response<bool>>
{
    private readonly IRepository<Producto> _productoRepository;
    private readonly IRepository<ProductoCompuesto> _productoCompuestoRepository;
    private readonly IMediator _mediator;

    public CrearMovimientoInventarioVentaCommandHandler(
        IRepository<Producto> productoRepository,
        IRepository<ProductoCompuesto> productoCompuestoRepository,
        IMediator mediator)
    {
        _productoRepository = productoRepository;
        _productoCompuestoRepository = productoCompuestoRepository;
        _mediator = mediator;
    }

    public async Task<Response<bool>> Handle(CrearMovimientoInventarioVentaCommand solicitud, CancellationToken tokenCancelacion)
    {
        var vendedor = (await _mediator.Send(new GetVendedorByIdQuery { Id = solicitud.IdVendedor }, tokenCancelacion)).Data;

        var idAlmacen = vendedor.ListaAlmacenes.FirstOrDefault()?.IdAlmacen ?? 0;

        var detallesInventario = await CrearDetallesInventarioAsync(solicitud.DetallesVenta, idAlmacen, tokenCancelacion);

        if (detallesInventario.Count == 0)
        {
            return new Response<bool>(true);
        }

        if (idAlmacen <= 0)
        {
            throw new InvalidOperationException($"El vendedor {vendedor.Id} no tiene un almacén asignado.");
        }

        await _mediator.Send(new CreateTransaccionInventarioCommand
        {
            TransaccionInventarioDTO = new TransaccionInventarioDTO
            {
                IdTransaccionInicial = solicitud.IdVenta,
                Glosa = "Salida por venta",
                Fecha = DateTime.Now,
                IdUsuario = vendedor.IdUsuario,
                Tipo = (short)TipoTransaccionInventario.Salida,
                Detalles = detallesInventario
            }
        }, tokenCancelacion);

        return new Response<bool>(true);
    }

    private async Task<List<TransaccionInventarioDetalleDTO>> CrearDetallesInventarioAsync(IEnumerable<VentaDetalleDTO> detallesVenta, long idAlmacen, CancellationToken tokenCancelacion)
    {
        var cantidades = new Dictionary<(long IdProducto, long IdAlmacen, long? IdLote), double>();
        var productos = new Dictionary<long, Producto>();
        var composiciones = new Dictionary<long, List<ProductoCompuesto>>();

        foreach (var detalle in detallesVenta)
        {
            if (detalle.Cantidad <= 0)
            {
                throw new InvalidOperationException($"La cantidad del producto {detalle.IdProducto} debe ser mayor a cero.");
            }

            var factorConversion = detalle.FactorConversion > 0 ? detalle.FactorConversion : 1;
            var cantidadBase = detalle.Cantidad * factorConversion;

            await ExpandirProductoAsync(
                detalle.IdProducto,
                (double)cantidadBase,
                idAlmacen,
                idLote: null,
                ruta: [],
                cantidades,
                productos,
                composiciones,
                tokenCancelacion);
        }

        return cantidades.Select(item => new TransaccionInventarioDetalleDTO
        {
            IdProducto = item.Key.IdProducto,
            IdAlmacen = item.Key.IdAlmacen,
            IdLote = item.Key.IdLote,
            Cantidad = item.Value
        }).ToList();
    }

    private async Task ExpandirProductoAsync(
        long idProducto,
        double cantidad,
        long idAlmacen,
        long? idLote,
        HashSet<long> ruta,
        IDictionary<(long IdProducto, long IdAlmacen, long? IdLote), double> cantidades,
        IDictionary<long, Producto> productos,
        IDictionary<long, List<ProductoCompuesto>> composiciones,
        CancellationToken tokenCancelacion)
    {
        if (!ruta.Add(idProducto))
        {
            throw new InvalidOperationException($"Se detectó una referencia circular en el producto compuesto {idProducto}.");
        }

        try
        {
            var producto = await ObtenerProductoAsync(idProducto, productos, tokenCancelacion);

            if (producto.Tipo == (short)TipoProducto.Servicio)
            {
                return;
            }

            if (!producto.EsCompuesto)
            {
                var clave = (idProducto, idAlmacen, idLote);
                cantidades.TryGetValue(clave, out var cantidadActual);
                cantidades[clave] = cantidadActual + cantidad;
                return;
            }

            var componentes = await ObtenerComponentesAsync(idProducto, composiciones, tokenCancelacion);

            foreach (var componente in componentes)
            {
                if (componente.Cantidad <= 0)
                {
                    throw new InvalidOperationException(
                        $"El componente {componente.IdProductoComponente} del producto " +
                        $"{idProducto} tiene una cantidad inválida.");
                }

                await ExpandirProductoAsync(
                    componente.IdProductoComponente,
                    cantidad * componente.Cantidad,
                    idAlmacen,
                    idLote,
                    ruta,
                    cantidades,
                    productos,
                    composiciones,
                    tokenCancelacion);
            }
        }
        finally
        {
            ruta.Remove(idProducto);
        }
    }

    private async Task<Producto> ObtenerProductoAsync(long idProducto, IDictionary<long, Producto> productos, CancellationToken tokenCancelacion)
    {
        if (productos.TryGetValue(idProducto, out var producto))
        {
            return producto;
        }

        producto = await _productoRepository.Query()
            .FirstOrDefaultAsync(
                item =>
                    !item.Eliminado &&
                    item.Activo &&
                    item.Id == idProducto,
                tokenCancelacion)
            ?? throw new InvalidOperationException($"No se encontró el producto activo {idProducto}.");

        productos[idProducto] = producto;
        return producto;
    }

    private async Task<List<ProductoCompuesto>> ObtenerComponentesAsync(long idProducto, IDictionary<long, List<ProductoCompuesto>> composiciones, CancellationToken tokenCancelacion)
    {
        if (composiciones.TryGetValue(idProducto, out var componentes))
        {
            return componentes;
        }

        componentes = await _productoCompuestoRepository.Query()
            .Where(componente =>
                !componente.Eliminado &&
                componente.IdProductoPadre == idProducto)
            .ToListAsync(tokenCancelacion);

        if (componentes.Count == 0)
        {
            throw new InvalidOperationException($"El producto compuesto {idProducto} no tiene componentes configurados.");
        }

        composiciones[idProducto] = componentes;
        return componentes;
    }
}
