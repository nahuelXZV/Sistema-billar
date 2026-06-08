using Application.Interfaces;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Inventory;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using static Domain.Constants.Constantes;
using Application.Helpers;

namespace Application.Features.Sales.OrdenMesas.Commands;

public class GuardarOrdenMesaCommand : ICommand<Response<OrdenMesaDTO>>
{
    public required OrdenMesaDTO OrdenMesa { get; set; }
}

public class GuardarOrdenMesaCommandHandler : ICommandHandler<GuardarOrdenMesaCommand, Response<OrdenMesaDTO>>
{
    private readonly IRepository<OrdenVenta> _ordenRepository;
    private readonly IRepository<OrdenVentaDetalle> _detalleRepository;
    private readonly IRepository<UsoMesa> _usoMesaRepository;
    private readonly IRepository<Producto> _productoRepository;

    public GuardarOrdenMesaCommandHandler(
        IRepository<OrdenVenta> ordenRepository,
        IRepository<OrdenVentaDetalle> detalleRepository,
        IRepository<UsoMesa> usoMesaRepository,
        IRepository<Producto> productoRepository)
    {
        _ordenRepository = ordenRepository;
        _detalleRepository = detalleRepository;
        _usoMesaRepository = usoMesaRepository;
        _productoRepository = productoRepository;
    }

    public async Task<Response<OrdenMesaDTO>> Handle(GuardarOrdenMesaCommand request, CancellationToken tokenCancelacion)
    {
        ValidarSolicitud(request.OrdenMesa);
        await ValidarProductosAsync(request.OrdenMesa.Detalles, tokenCancelacion);

        var ahora = DateTime.Now;
        OrdenVenta orden;
        UsoMesa usoMesa;

        if (request.OrdenMesa.IdOrdenVenta <= 0)
        {
            var mesaOcupada = await (
                from uso in _usoMesaRepository.Query()
                join ordenActual in _ordenRepository.Query()
                    on uso.IdOrdenVenta equals ordenActual.Id
                where !uso.Eliminado &&
                      uso.IdMesa == request.OrdenMesa.IdMesa &&
                      !ordenActual.Eliminado &&
                      ordenActual.Estado == (short)EstadoOrdenVenta.Abierta
                select uso.Id)
                .AnyAsync(tokenCancelacion);

            if (mesaOcupada)
            {
                throw new InvalidOperationException("La mesa ya tiene una orden abierta.");
            }

            orden = new OrdenVenta
            {
                IdCliente = request.OrdenMesa.IdCliente,
                Numero = string.Empty,
                Estado = (short)EstadoOrdenVenta.Abierta,
                FechaApertura = ahora,
                Observacion = request.OrdenMesa.Observacion
            };

            AplicarTotales(orden, request.OrdenMesa);
            orden = await _ordenRepository.AddAsync(orden);
            await _ordenRepository.UnitOfWork.SaveEntitiesAsync(tokenCancelacion);

            orden.Numero = $"OV-{orden.FechaApertura:yyyyMMdd}-{orden.Id:D8}";
            _ordenRepository.Update(orden);

            usoMesa = new UsoMesa
            {
                IdOrdenVenta = orden.Id,
                IdMesa = request.OrdenMesa.IdMesa,
                FechaInicio = ahora,
                MinutosConsumidos = 0,
                TarifaAplicada = Convert.ToDouble(request.OrdenMesa.TarifaAplicada),
                MontoCalculado = 0,
                Estado = (short)EstadoUsoMesa.Pendiente
            };

            usoMesa = await _usoMesaRepository.AddAsync(usoMesa);
            await _usoMesaRepository.UnitOfWork.SaveEntitiesAsync(tokenCancelacion);
        }
        else
        {
            orden = await _ordenRepository.Query().FirstOrDefaultAsync(
                    ordenActual => !ordenActual.Eliminado && ordenActual.Id == request.OrdenMesa.IdOrdenVenta,
                    tokenCancelacion)
                ?? throw new InvalidOperationException("La orden de mesa no existe.");

            if (orden.Estado != (short)EstadoOrdenVenta.Abierta)
            {
                throw new InvalidOperationException("La orden de mesa ya está cerrada.");
            }

            usoMesa = await _usoMesaRepository.Query()
                .FirstOrDefaultAsync(
                    uso => !uso.Eliminado &&
                           uso.IdOrdenVenta == orden.Id &&
                           uso.IdMesa == request.OrdenMesa.IdMesa,
                    tokenCancelacion)
                ?? throw new InvalidOperationException("El uso de la mesa no existe.");

            orden.IdCliente = request.OrdenMesa.IdCliente;
            orden.Observacion = request.OrdenMesa.Observacion;
            AplicarTotales(orden, request.OrdenMesa);
            _ordenRepository.Update(orden);

            usoMesa.TarifaAplicada = Convert.ToDouble(request.OrdenMesa.TarifaAplicada);
            ActualizarTiempo(usoMesa, ahora);
            _usoMesaRepository.Update(usoMesa);

            var detallesAnteriores = await _detalleRepository.Query()
                .Where(detalle => !detalle.Eliminado && detalle.IdOrdenVenta == orden.Id)
                .ToListAsync(tokenCancelacion);

            if (detallesAnteriores.Count > 0)
            {
                _detalleRepository.DeleteRange(detallesAnteriores);
            }
        }

        var detalles = CrearDetalles(request.OrdenMesa, orden.Id, usoMesa.Id);
        if (detalles.Count > 0)
        {
            await _detalleRepository.AddRangeAsync(detalles);
        }

        await _ordenRepository.UnitOfWork.SaveEntitiesAsync(tokenCancelacion);
        return new Response<OrdenMesaDTO>(OrdenMesaMapeo.Crear(orden, usoMesa, detalles));
    }

    private static void ValidarSolicitud(OrdenMesaDTO ordenMesa)
    {
        if (ordenMesa.IdMesa <= 0)
            throw new InvalidOperationException("Debe seleccionar una mesa.");

        if (ordenMesa.IdVendedor <= 0)
            throw new InvalidOperationException("Debe existir un vendedor para guardar la orden.");

        if (ordenMesa.DescuentoGlobal < 0 || ordenMesa.RecargoGlobal < 0)
            throw new InvalidOperationException("El descuento y el recargo no pueden ser negativos.");

        foreach (var detalle in ordenMesa.Detalles)
        {
            if (detalle.IdProducto <= 0 || detalle.Cantidad <= 0 || detalle.PrecioUnitario < 0)
                throw new InvalidOperationException("Los detalles de la orden contienen valores inválidos.");

            if (detalle.Descuento < 0)
                throw new InvalidOperationException("El descuento de un detalle no puede ser negativo.");
        }
    }

    private async Task ValidarProductosAsync(IEnumerable<OrdenMesaDetalleDTO> detalles, CancellationToken tokenCancelacion)
    {
        var ids = detalles.Select(detalle => detalle.IdProducto).Distinct().ToList();
        if (ids.Count == 0) return;

        var cantidadProductosValidos = await _productoRepository.Query()
            .CountAsync(producto => ids.Contains(producto.Id) && producto.Activo && !producto.Eliminado,
                tokenCancelacion);

        if (cantidadProductosValidos != ids.Count)
            throw new InvalidOperationException("La orden contiene productos inexistentes o inactivos.");
    }

    private static List<OrdenVentaDetalle> CrearDetalles(OrdenMesaDTO solicitud, long idOrdenVenta, long idUsoMesa)
    {
        return solicitud.Detalles.Select(detalle =>
        {
            var subTotal = Redondear(detalle.Cantidad * detalle.PrecioUnitario);
            if (detalle.Descuento > subTotal)
                throw new InvalidOperationException("El descuento no puede superar el subtotal del detalle.");

            return new OrdenVentaDetalle
            {
                IdOrdenVenta = idOrdenVenta,
                IdProducto = detalle.IdProducto,
                IdUsoMesa = detalle.EsTiempoMesa ? idUsoMesa : null,
                IdVendedor = solicitud.IdVendedor,
                NombreProducto = detalle.NombreProducto,
                Cantidad = detalle.Cantidad,
                PrecioUnitario = detalle.PrecioUnitario,
                Descuento = detalle.Descuento,
                SubTotal = subTotal,
                Total = Redondear(subTotal - detalle.Descuento),
                Estado = (short)EstadoOrdenVenta.Abierta
            };
        }).ToList();
    }

    private static void AplicarTotales(OrdenVenta orden, OrdenMesaDTO solicitud)
    {
        var subTotalProductos = solicitud.Detalles
            .Where(detalle => !detalle.EsTiempoMesa)
            .Sum(detalle => Redondear(detalle.Cantidad * detalle.PrecioUnitario - detalle.Descuento));

        var subTotalTiempo = solicitud.Detalles
            .Where(detalle => detalle.EsTiempoMesa)
            .Sum(detalle => Redondear(detalle.Cantidad * detalle.PrecioUnitario - detalle.Descuento));

        var totalAntesDescuento = Redondear(subTotalProductos + subTotalTiempo);
        if (solicitud.DescuentoGlobal > totalAntesDescuento)
            throw new InvalidOperationException("El descuento no puede superar el subtotal de la orden.");

        orden.SubTotalProductos = Redondear(subTotalProductos);
        orden.SubTotalTiempo = Redondear(subTotalTiempo);
        orden.DescuentoGlobal = Redondear(solicitud.DescuentoGlobal);
        orden.RecargoGlobal = Redondear(solicitud.RecargoGlobal);
        orden.Total = Redondear(totalAntesDescuento - orden.DescuentoGlobal + orden.RecargoGlobal);
        orden.SaldoPendiente = orden.Total;
    }

    private static void ActualizarTiempo(UsoMesa usoMesa, DateTime ahora)
    {
        if (usoMesa.Estado != (short)EstadoUsoMesa.EnCurso) return;

        usoMesa.MinutosConsumidos = Math.Max(0, (ahora - usoMesa.FechaInicio).TotalMinutes);
        usoMesa.MontoCalculado = usoMesa.MinutosConsumidos / 60 * usoMesa.TarifaAplicada;
    }

    private static decimal Redondear(decimal valor) => Math.Round(valor, 2, MidpointRounding.AwayFromZero);
}
