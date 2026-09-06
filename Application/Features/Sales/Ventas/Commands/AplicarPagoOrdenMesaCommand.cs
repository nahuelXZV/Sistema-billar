using Application.Interfaces;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Domain.Utils;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using static Domain.Constants.Constantes;

namespace Application.Features.Sales.Ventas.Commands;

public class AplicarPagoOrdenMesaCommand : ICommand<Response<bool>>
{
    public long IdOrdenVenta { get; set; }
    public decimal TotalVenta { get; set; }
    public bool FinalizarOrdenVenta { get; set; }
    public IReadOnlyCollection<VentaDetalleDTO> DetallesPagados { get; set; } = [];
}

public class AplicarPagoOrdenMesaCommandHandler : ICommandHandler<AplicarPagoOrdenMesaCommand, Response<bool>>
{
    private readonly IRepository<OrdenVenta> _ordenRepository;
    private readonly IRepository<OrdenVentaDetalle> _detalleRepository;
    private readonly IRepository<UsoMesa> _usoMesaRepository;

    public AplicarPagoOrdenMesaCommandHandler(IRepository<OrdenVenta> ordenRepository, IRepository<OrdenVentaDetalle> detalleRepository, IRepository<UsoMesa> usoMesaRepository)
    {
        _ordenRepository = ordenRepository;
        _detalleRepository = detalleRepository;
        _usoMesaRepository = usoMesaRepository;
    }

    public async Task<Response<bool>> Handle(AplicarPagoOrdenMesaCommand solicitud, CancellationToken tokenCancelacion)
    {
        var orden = await _ordenRepository.Query().FirstOrDefaultAsync(item => !item.Eliminado && item.Id == solicitud.IdOrdenVenta, tokenCancelacion)
            ?? throw new InvalidOperationException("La orden asociada a la venta no existe.");

        if (orden.Estado != (short)EstadoOrdenVenta.Abierta)
        {
            throw new InvalidOperationException("La orden asociada a la venta ya está cerrada.");
        }

        var usoMesa = await _usoMesaRepository.Query()
            .FirstOrDefaultAsync(uso => !uso.Eliminado && uso.IdOrdenVenta == solicitud.IdOrdenVenta, tokenCancelacion)
            ?? throw new InvalidOperationException("El uso asociado a la orden no existe.");

        var detallesOrden = await _detalleRepository.Query()
            .Where(detalle => !detalle.Eliminado && detalle.IdOrdenVenta == solicitud.IdOrdenVenta)
            .ToListAsync(tokenCancelacion);

        foreach (var detallePagado in solicitud.DetallesPagados)
        {
            AplicarPagoDetalle(detallePagado, detallesOrden, usoMesa);
        }

        RecalcularOrden(orden, detallesOrden, solicitud.TotalVenta);
        if (solicitud.FinalizarOrdenVenta)
        {
            FinalizarOrden(orden, usoMesa, detallesOrden);
        }

        _ordenRepository.Update(orden);
        await _ordenRepository.UnitOfWork.SaveEntitiesAsync(tokenCancelacion);

        return new Response<bool>(true);
    }

    private void AplicarPagoDetalle(VentaDetalleDTO detallePagado, List<OrdenVentaDetalle> detallesOrden, UsoMesa usoMesa)
    {
        if (!detallePagado.IdOrdenVentaDetalle.HasValue)
        {
            throw new InvalidOperationException($"El producto {detallePagado.IdProducto} no está asociado a un detalle de la orden.");
        }

        var detalleOrden = detallesOrden.FirstOrDefault(detalle => detalle.Id == detallePagado.IdOrdenVentaDetalle.Value)
            ?? throw new InvalidOperationException($"El detalle {detallePagado.IdOrdenVentaDetalle.Value} no pertenece a la orden.");

        if (detalleOrden.IdUsoMesa.HasValue && usoMesa.Estado == (short)EstadoUsoMesa.EnCurso)
        {
            throw new InvalidOperationException("Debe finalizar el cronómetro antes de pagar el tiempo de la mesa.");
        }

        if (detallePagado.Cantidad <= 0 || detallePagado.Cantidad > detalleOrden.Cantidad)
        {
            throw new InvalidOperationException($"La cantidad pagada del producto {detallePagado.IdProducto} supera la cantidad pendiente.");
        }

        detalleOrden.Cantidad = Utils.Redondear(detalleOrden.Cantidad - detallePagado.Cantidad);

        if (detalleOrden.Cantidad <= 0)
        {
            _detalleRepository.Delete(detalleOrden);
            detallesOrden.Remove(detalleOrden);
            return;
        }

        detalleOrden.SubTotal = Utils.Redondear(detalleOrden.Cantidad * detalleOrden.PrecioUnitario);
        detalleOrden.Descuento = Math.Min(detalleOrden.Descuento, detalleOrden.SubTotal);
        detalleOrden.Total = Utils.Redondear(detalleOrden.SubTotal - detalleOrden.Descuento);

        _detalleRepository.Update(detalleOrden);
    }

    private static void RecalcularOrden(OrdenVenta orden, IReadOnlyCollection<OrdenVentaDetalle> detallesOrden, decimal totalVenta)
    {
        orden.SubTotalProductos = Utils.Redondear(detallesOrden
            .Where(detalle => !detalle.IdUsoMesa.HasValue)
            .Sum(detalle => detalle.Total));
        orden.SubTotalTiempo = Utils.Redondear(detallesOrden
            .Where(detalle => detalle.IdUsoMesa.HasValue)
            .Sum(detalle => detalle.Total));
        orden.DescuentoGlobal = 0;
        orden.RecargoGlobal = 0;
        orden.Total = Utils.Redondear(orden.SubTotalProductos + orden.SubTotalTiempo);
        orden.TotalPagado = Utils.Redondear(orden.TotalPagado + totalVenta);
        orden.SaldoPendiente = orden.Total;
    }

    private void FinalizarOrden(OrdenVenta orden, UsoMesa usoMesa, IReadOnlyCollection<OrdenVentaDetalle> detallesOrden)
    {
        if (detallesOrden.Count > 0)
        {
            throw new InvalidOperationException("No se puede finalizar la orden porque aún tiene detalles pendientes de pago.");
        }

        if (usoMesa.Estado == (short)EstadoUsoMesa.EnCurso)
        {
            throw new InvalidOperationException("Debe finalizar el cronómetro antes de finalizar la orden.");
        }

        var ahora = DateTime.Now;
        orden.FechaCierre = ahora;
        orden.Estado = (short)EstadoOrdenVenta.Cerrada;
        orden.SaldoPendiente = 0;

        if (usoMesa.Estado == (short)EstadoUsoMesa.Finalizado)
        {
            return;
        }

        usoMesa.FechaFin = ahora;
        usoMesa.Estado = (short)EstadoUsoMesa.Finalizado;
        _usoMesaRepository.Update(usoMesa);
    }

}
