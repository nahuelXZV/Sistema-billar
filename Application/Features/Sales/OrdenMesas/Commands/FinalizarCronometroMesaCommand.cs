using Application.Helpers;
using Application.Interfaces;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using static Domain.Constants.Constantes;

namespace Application.Features.Sales.OrdenMesas.Commands;

public class FinalizarCronometroMesaCommand : ICommand<Response<OrdenMesaDTO>>
{
    public long IdOrdenVenta { get; set; }
}

public class FinalizarCronometroMesaCommandHandler : ICommandHandler<FinalizarCronometroMesaCommand, Response<OrdenMesaDTO>>
{
    private readonly IRepository<OrdenVenta> _ordenRepository;
    private readonly IRepository<OrdenVentaDetalle> _detalleRepository;
    private readonly IRepository<UsoMesa> _usoMesaRepository;

    public FinalizarCronometroMesaCommandHandler(
        IRepository<OrdenVenta> ordenRepository,
        IRepository<OrdenVentaDetalle> detalleRepository,
        IRepository<UsoMesa> usoMesaRepository)
    {
        _ordenRepository = ordenRepository;
        _detalleRepository = detalleRepository;
        _usoMesaRepository = usoMesaRepository;
    }

    public async Task<Response<OrdenMesaDTO>> Handle(FinalizarCronometroMesaCommand request, CancellationToken tokenCancelacion)
    {
        var orden = await _ordenRepository.Query()
            .FirstOrDefaultAsync(
                ordenActual => !ordenActual.Eliminado &&
                               ordenActual.Id == request.IdOrdenVenta &&
                               ordenActual.Estado == (short)EstadoOrdenVenta.Abierta,
                tokenCancelacion)
            ?? throw new InvalidOperationException("La orden de mesa no existe o ya fue cerrada.");

        var usoMesa = await _usoMesaRepository.Query()
            .FirstOrDefaultAsync(
                uso => !uso.Eliminado && uso.IdOrdenVenta == orden.Id,
                tokenCancelacion)
            ?? throw new InvalidOperationException("El uso asociado a la mesa no existe.");

        if (usoMesa.Estado != (short)EstadoUsoMesa.EnCurso)
        {
            throw new InvalidOperationException("El cronometro de la mesa no esta en curso.");
        }

        var ahora = DateTime.Now;
        usoMesa.MinutosConsumidos = Math.Max(0, (ahora - usoMesa.FechaInicio).TotalMinutes);
        usoMesa.MontoCalculado = usoMesa.MinutosConsumidos / 60 * usoMesa.TarifaAplicada;
        usoMesa.FechaFin = ahora;
        usoMesa.Estado = (short)EstadoUsoMesa.Finalizado;
        _usoMesaRepository.Update(usoMesa);

        var detalles = await _detalleRepository.Query()
            .Where(detalle => !detalle.Eliminado && detalle.IdOrdenVenta == orden.Id)
            .ToListAsync(tokenCancelacion);

        var detalleTiempo = detalles.FirstOrDefault(detalle => detalle.IdUsoMesa == usoMesa.Id);
        if (usoMesa.TarifaAplicada > 0 && detalleTiempo is null)
        {
            throw new InvalidOperationException("Debe guardar el detalle de tiempo antes de finalizar el cronometro.");
        }

        if (detalleTiempo is not null)
        {
            detalleTiempo.Cantidad = Math.Max(
                0.01m,
                Redondear((decimal)usoMesa.MinutosConsumidos / 60));
            detalleTiempo.SubTotal = Redondear(
                detalleTiempo.Cantidad * detalleTiempo.PrecioUnitario);
            detalleTiempo.Descuento = Math.Min(detalleTiempo.Descuento, detalleTiempo.SubTotal);
            detalleTiempo.Total = Redondear(detalleTiempo.SubTotal - detalleTiempo.Descuento);
            _detalleRepository.Update(detalleTiempo);
        }

        orden.SubTotalProductos = Redondear(detalles
            .Where(detalle => !detalle.IdUsoMesa.HasValue)
            .Sum(detalle => detalle.Total));
        orden.SubTotalTiempo = Redondear(detalles
            .Where(detalle => detalle.IdUsoMesa.HasValue)
            .Sum(detalle => detalle.Total));
        orden.Total = Redondear(
            orden.SubTotalProductos +
            orden.SubTotalTiempo -
            orden.DescuentoGlobal +
            orden.RecargoGlobal);
        orden.SaldoPendiente = orden.Total;
        _ordenRepository.Update(orden);

        await _usoMesaRepository.UnitOfWork.SaveEntitiesAsync(tokenCancelacion);
        return new Response<OrdenMesaDTO>(OrdenMesaMapeo.Crear(orden, usoMesa, detalles));
    }

    private static decimal Redondear(decimal valor) => Math.Round(valor, 2, MidpointRounding.AwayFromZero);
}
