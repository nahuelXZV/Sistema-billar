using Application.Interfaces;
using Application.Common.Utils;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using static Domain.Constants.Constantes;

namespace Application.Features.Sales.OrdenMesas.Commands;

public class EliminarOrdenMesaCommand : ICommand<Response<ResultadoEliminarOrdenMesaDTO>>
{
    public long IdOrdenVenta { get; set; }
}

public class EliminarOrdenMesaCommandHandler : ICommandHandler<EliminarOrdenMesaCommand, Response<ResultadoEliminarOrdenMesaDTO>>
{
    private readonly IRepository<OrdenVenta> _ordenRepository;
    private readonly IRepository<OrdenVentaDetalle> _detalleRepository;
    private readonly IRepository<UsoMesa> _usoMesaRepository;
    private readonly IRepository<Venta> _ventaRepository;

    public EliminarOrdenMesaCommandHandler(
        IRepository<OrdenVenta> ordenRepository,
        IRepository<OrdenVentaDetalle> detalleRepository,
        IRepository<UsoMesa> usoMesaRepository,
        IRepository<Venta> ventaRepository)
    {
        _ordenRepository = ordenRepository;
        _detalleRepository = detalleRepository;
        _usoMesaRepository = usoMesaRepository;
        _ventaRepository = ventaRepository;
    }

    public async Task<Response<ResultadoEliminarOrdenMesaDTO>> Handle(EliminarOrdenMesaCommand request, CancellationToken cancellationToken)
    {
        if (request.IdOrdenVenta <= 0)
        {
            throw new InvalidOperationException("Debe seleccionar una orden de mesa válida.");
        }

        var orden = await _ordenRepository.Query()
            .FirstOrDefaultAsync(
                ordenActual => !ordenActual.Eliminado && ordenActual.Id == request.IdOrdenVenta,
                cancellationToken)
            ?? throw new InvalidOperationException("La orden de mesa no existe.");

        if (orden.Estado != (short)EstadoOrdenVenta.Abierta)
        {
            throw new InvalidOperationException("Solo se pueden eliminar órdenes de mesa abiertas.");
        }

        var tieneVentasRegistradas = orden.TotalPagado > 0 ||
            await _ventaRepository.Query().AnyAsync(
                venta => !venta.Eliminado && venta.IdOrdenVenta == orden.Id,
                cancellationToken);

        var detalles = await _detalleRepository.Query()
            .Where(detalle => !detalle.Eliminado && detalle.IdOrdenVenta == orden.Id)
            .ToListAsync(cancellationToken);

        var usosMesa = await _usoMesaRepository.Query()
            .Where(uso => !uso.Eliminado && uso.IdOrdenVenta == orden.Id)
            .ToListAsync(cancellationToken);

        if (tieneVentasRegistradas)
        {
            FinalizarOrden(orden, detalles, usosMesa);
            await _ordenRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

            return new Response<ResultadoEliminarOrdenMesaDTO>(new ResultadoEliminarOrdenMesaDTO
            {
                Finalizada = true
            });
        }

        if (detalles.Count > 0)
        {
            _detalleRepository.DeleteRange(detalles);
        }

        if (usosMesa.Count > 0)
        {
            _usoMesaRepository.DeleteRange(usosMesa);
        }

        _ordenRepository.Delete(orden);
        await _ordenRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<ResultadoEliminarOrdenMesaDTO>(new ResultadoEliminarOrdenMesaDTO
        {
            Eliminada = true
        });
    }

    private void FinalizarOrden(OrdenVenta orden, IReadOnlyCollection<OrdenVentaDetalle> detalles, IReadOnlyCollection<UsoMesa> usosMesa)
    {
        var ahora = DateTime.Now;

        orden.Estado = (short)EstadoOrdenVenta.Cerrada;
        orden.FechaCierre = ahora;
        orden.SaldoPendiente = 0;
        _ordenRepository.Update(orden);

        foreach (var detalle in detalles)
        {
            detalle.Estado = (short)EstadoOrdenVenta.Cerrada;
        }

        if (detalles.Count > 0)
        {
            _detalleRepository.UpdateRange(detalles);
        }

        foreach (var usoMesa in usosMesa)
        {
            OrdenMesaUtils.ActualizarTiempo(usoMesa, ahora);
            usoMesa.FechaFin = ahora;
            usoMesa.Estado = (short)EstadoUsoMesa.Finalizado;
            usoMesa.Eliminado = true;
        }

        if (usosMesa.Count > 0)
        {
            _usoMesaRepository.UpdateRange(usosMesa);
        }
    }
}
