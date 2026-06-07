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
    private readonly IMediator _mediator;

    public CreateVentaCommandHandler(
        IMapper mapper,
        IRepository<Venta> repository,
        IRepository<Producto> productoRepository,
        IRepository<ProductoCompuesto> productoCompuestoRepository,
        IMediator mediator)
    {
        _mapper = mapper;
        _repository = repository;
        _productoRepository = productoRepository;
        _productoCompuestoRepository = productoCompuestoRepository;
        _mediator = mediator;
    }

    public async Task<Response<long>> Handle(CreateVentaCommand request, CancellationToken cancellationToken)
    {
        Venta venta = _mapper.Map<Venta>(request.VentaDTO);
        venta.Id = 0;
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
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

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
                producto = await _productoRepository.Query().Where(p => !p.Eliminado && p.Id == idProducto).FirstOrDefaultAsync(cancellationToken)
                    ?? throw new InvalidOperationException($"No se encontró el producto {idProducto}.");

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
