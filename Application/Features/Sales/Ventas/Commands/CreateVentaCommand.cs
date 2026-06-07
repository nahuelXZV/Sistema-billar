using Application.Features.Inventory.TransaccionInventarios.Command;
using Application.Features.Sales.Vendedores.Queries;
using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.DTOs.Sales;
using Domain.Entities.Inventory;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static Domain.Constants.Constantes;

namespace Application.Features.Sales.Ventas.Commands;

public class CreateVentaCommand : ICommand<Response<long>>
{
    public required VentaDTO VentaDTO { get; set; }
}

public class CreateVentaCommandHandler : ICommandHandler<CreateVentaCommand, Response<long>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Venta> _repository;
    private readonly IRepository<Producto> _productoRepository;
    private readonly IRepository<ProductoCompuesto> _productoCompuestoRepository;
    private readonly IDbContext _dbContext;
    private readonly IMediator _mediator;

    public CreateVentaCommandHandler(
        IMapper mapper,
        IRepository<Venta> repository,
        IRepository<Producto> productoRepository,
        IRepository<ProductoCompuesto> productoCompuestoRepository,
        IDbContext dbContext,
        IMediator mediator)
    {
        _mapper = mapper;
        _repository = repository;
        _productoRepository = productoRepository;
        _productoCompuestoRepository = productoCompuestoRepository;
        _dbContext = dbContext;
        _mediator = mediator;
    }

    public async Task<Response<long>> Handle(CreateVentaCommand request, CancellationToken cancellationToken)
    {
        var idempotencyKey = request.VentaDTO.IdempotencyKey;
        if (!idempotencyKey.HasValue || idempotencyKey == Guid.Empty)
        {
            throw new InvalidOperationException("La venta debe incluir una clave de idempotencia válida.");
        }

        var idVentaExistente = await ObtenerIdVentaPorIdempotenciaAsync(idempotencyKey.Value, cancellationToken);
        if (idVentaExistente > 0)
        {
            return new Response<long>(idVentaExistente);
        }

        ValidarImportesVenta(request.VentaDTO);
        await ValidarProductosVentaAsync(request.VentaDTO.ListaDetalles ?? [], cancellationToken);

        Venta venta = _mapper.Map<Venta>(request.VentaDTO);
        venta.Id = 0;
        venta.IdempotencyKey = idempotencyKey.Value;
        venta.Numero = string.Empty;
        venta.ListaDetalles = _mapper.Map<List<VentaDetalle>>(request.VentaDTO.ListaDetalles ?? []);
        venta.ListaPagos = _mapper.Map<List<PagoVenta>>(request.VentaDTO.ListaPagos ?? []);

        if (venta.IdOrdenVenta == 0) venta.IdOrdenVenta = null;
        foreach (var detalle in venta.ListaDetalles)
        {
            detalle.Id = 0;
            detalle.IdVenta = 0;
            if (detalle.IdOrdenVentaDetalle == 0) detalle.IdOrdenVentaDetalle = null;
        }
        foreach (var pago in venta.ListaPagos)
        {
            pago.Id = 0;
            pago.IdVenta = 0;
        }

        venta = await _repository.AddAsync(venta);
        try
        {
            await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            _dbContext.dbContext.ChangeTracker.Clear();

            idVentaExistente = await ObtenerIdVentaPorIdempotenciaAsync(idempotencyKey.Value, cancellationToken);
            if (idVentaExistente > 0)
            {
                return new Response<long>(idVentaExistente);
            }

            throw;
        }

        venta.Numero = $"V-{venta.Fecha:yyyyMMdd}-{venta.Id:D8}";
        _repository.Update(venta);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        // Crear Movimiento Productos
        var vendedor = (await _mediator.Send(new GetVendedorByIdQuery { Id = request.VentaDTO.IdVendedor }, cancellationToken)).Data;
        var idAlmacen = vendedor.ListaAlmacenes.FirstOrDefault()?.IdAlmacen ?? 0;
        var detallesInventario = await CrearDetallesInventarioAsync(request.VentaDTO.ListaDetalles ?? [], idAlmacen, cancellationToken);

        if (detallesInventario.Count > 0)
        {
            if (idAlmacen <= 0)
            {
                throw new InvalidOperationException($"El vendedor {vendedor.Id} no tiene un almacén asignado.");
            }

            await _mediator.Send(new CreateTransaccionInventarioCommand()
            {
                TransaccionInventarioDTO = new()
                {
                    IdTransaccionInicial = venta.Id,
                    Glosa = "Salida por venta",
                    Fecha = DateTime.Now,
                    IdUsuario = vendedor.IdUsuario,
                    Tipo = (short)TipoTransaccionInventario.Salida,
                    Detalles = detallesInventario
                }
            }, cancellationToken);
        }

        return new Response<long>(venta.Id);
    }

    private async Task<long> ObtenerIdVentaPorIdempotenciaAsync(
        Guid idempotencyKey,
        CancellationToken cancellationToken)
    {
        return await _repository.Query()
            .Where(venta => venta.IdempotencyKey == idempotencyKey)
            .Select(venta => venta.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static void ValidarImportesVenta(VentaDTO venta)
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

        decimal subTotalCalculado = 0;
        decimal totalDetallesCalculado = 0;

        foreach (var detalle in detalles)
        {
            if (detalle.IdProducto <= 0)
            {
                throw new InvalidOperationException("Todos los detalles deben tener un producto válido.");
            }

            if (detalle.Cantidad <= 0 || detalle.PrecioUnitario < 0)
            {
                throw new InvalidOperationException(
                    $"La cantidad y el precio del producto {detalle.IdProducto} no son válidos.");
            }

            var descuentoDetalle = detalle.Descuento ?? 0;
            if (descuentoDetalle < 0)
            {
                throw new InvalidOperationException(
                    $"El descuento del producto {detalle.IdProducto} no puede ser negativo.");
            }

            var subTotalDetalle = Redondear(detalle.Cantidad * detalle.PrecioUnitario);
            if (descuentoDetalle > subTotalDetalle)
            {
                throw new InvalidOperationException(
                    $"El descuento del producto {detalle.IdProducto} no puede superar su subtotal.");
            }

            var totalDetalle = Redondear(subTotalDetalle - descuentoDetalle);
            if (!Coincide(detalle.SubTotal, subTotalDetalle) ||
                !Coincide(detalle.Total, totalDetalle))
            {
                throw new InvalidOperationException(
                    $"Los importes del producto {detalle.IdProducto} no coinciden con su cantidad, precio y descuento.");
            }

            subTotalCalculado += subTotalDetalle;
            totalDetallesCalculado += totalDetalle;
        }

        subTotalCalculado = Redondear(subTotalCalculado);
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

        if (!Coincide(venta.SubTotal, subTotalCalculado) ||
            !Coincide(venta.Total, totalCalculado))
        {
            throw new InvalidOperationException(
                "El subtotal o el total de la venta no coincide con sus detalles, descuento y recargo.");
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

    private static decimal Redondear(decimal valor) =>
        Math.Round(valor, 2, MidpointRounding.AwayFromZero);

    private static bool Coincide(decimal valorRecibido, decimal valorCalculado) =>
        Redondear(valorRecibido) == Redondear(valorCalculado);

    private async Task ValidarProductosVentaAsync(
        IEnumerable<VentaDetalleDTO> detalles,
        CancellationToken cancellationToken)
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
            .ToListAsync(cancellationToken);

        var idsProductosInvalidos = idsProductos
            .Except(productosValidos)
            .ToList();

        if (idsProductosInvalidos.Count > 0)
        {
            throw new InvalidOperationException(
                $"Los siguientes productos no existen o están inactivos: {string.Join(", ", idsProductosInvalidos)}.");
        }
    }

    private async Task<List<TransaccionInventarioDetalleDTO>> CrearDetallesInventarioAsync(
        IEnumerable<VentaDetalleDTO> detallesVenta,
        long idAlmacen,
        CancellationToken cancellationToken)
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

            await ExpandirProductoAsync(
                detalle.IdProducto,
                (double)detalle.Cantidad,
                idAlmacen,
                idLote: null,
                ruta: [],
                cantidades,
                productos,
                composiciones,
                cancellationToken);
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
        CancellationToken cancellationToken)
    {
        if (!ruta.Add(idProducto))
        {
            throw new InvalidOperationException($"Se detectó una referencia circular en el producto compuesto {idProducto}.");
        }

        try
        {
            if (!productos.TryGetValue(idProducto, out var producto))
            {
                producto = await _productoRepository.Query()
                    .Where(p => !p.Eliminado && p.Activo && p.Id == idProducto)
                    .FirstOrDefaultAsync(cancellationToken)
                    ?? throw new InvalidOperationException(
                        $"No se encontró el producto activo {idProducto}.");

                productos[idProducto] = producto;
            }

            if (producto.Tipo == (short)TipoProducto.Servicio)
            {
                return;
            }

            if (!producto.EsCompuesto)
            {
                var key = (idProducto, idAlmacen, idLote);
                cantidades.TryGetValue(key, out var cantidadActual);
                cantidades[key] = cantidadActual + cantidad;
                return;
            }

            if (!composiciones.TryGetValue(idProducto, out var componentes))
            {
                componentes = await _productoCompuestoRepository.Query()
                    .Where(pc => !pc.Eliminado && pc.IdProductoPadre == idProducto)
                    .ToListAsync(cancellationToken);

                composiciones[idProducto] = componentes;
            }

            if (componentes.Count == 0)
            {
                throw new InvalidOperationException($"El producto compuesto {idProducto} no tiene componentes configurados.");
            }

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
                    cancellationToken);
            }
        }
        finally
        {
            ruta.Remove(idProducto);
        }
    }
}
