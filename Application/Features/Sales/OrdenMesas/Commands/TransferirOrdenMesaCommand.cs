using Application.Common.Utils;
using Application.Interfaces;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Configuration;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using static Domain.Constants.Constantes;

namespace Application.Features.Sales.OrdenMesas.Commands;

public class TransferirOrdenMesaCommand : ICommand<Response<OrdenMesaDTO>>
{
    public required TransferirOrdenMesaDTO Transferencia { get; set; }
}

public class TransferirOrdenMesaCommandHandler : ICommandHandler<TransferirOrdenMesaCommand, Response<OrdenMesaDTO>>
{
    private readonly IRepository<OrdenVenta> _ordenRepository;
    private readonly IRepository<OrdenVentaDetalle> _detalleRepository;
    private readonly IRepository<UsoMesa> _usoMesaRepository;
    private readonly IRepository<Mesa> _mesaRepository;

    public TransferirOrdenMesaCommandHandler(
        IRepository<OrdenVenta> ordenRepository,
        IRepository<OrdenVentaDetalle> detalleRepository,
        IRepository<UsoMesa> usoMesaRepository,
        IRepository<Mesa> mesaRepository)
    {
        _ordenRepository = ordenRepository;
        _detalleRepository = detalleRepository;
        _usoMesaRepository = usoMesaRepository;
        _mesaRepository = mesaRepository;
    }

    public async Task<Response<OrdenMesaDTO>> Handle(TransferirOrdenMesaCommand request, CancellationToken cancellationToken)
    {
        ValidarSolicitud(request.Transferencia);

        var orden = await _ordenRepository.Query()
            .FirstOrDefaultAsync(
                ordenActual => !ordenActual.Eliminado && ordenActual.Id == request.Transferencia.IdOrdenVenta,
                cancellationToken)
            ?? throw new InvalidOperationException("La orden de mesa no existe.");

        if (orden.Estado != (short)EstadoOrdenVenta.Abierta)
        {
            throw new InvalidOperationException("Solo se pueden transferir órdenes de mesa abiertas.");
        }

        var usoMesa = await _usoMesaRepository.Query()
            .FirstOrDefaultAsync(
                uso => !uso.Eliminado && uso.IdOrdenVenta == orden.Id,
                cancellationToken)
            ?? throw new InvalidOperationException("El uso asociado a la orden no existe.");

        if (usoMesa.IdMesa == request.Transferencia.IdMesaDestino)
        {
            throw new InvalidOperationException("La mesa de destino debe ser diferente a la mesa actual.");
        }

        var mesaDestino = await _mesaRepository.Query()
            .FirstOrDefaultAsync(
                mesa => !mesa.Eliminado && mesa.Id == request.Transferencia.IdMesaDestino,
                cancellationToken)
            ?? throw new InvalidOperationException("La mesa de destino no existe.");

        if (!mesaDestino.Activo)
        {
            throw new InvalidOperationException("La mesa de destino está inactiva.");
        }

        var mesaDestinoOcupada = await (
            from uso in _usoMesaRepository.Query()
            join ordenActual in _ordenRepository.Query() on uso.IdOrdenVenta equals ordenActual.Id
            where !uso.Eliminado &&
                  uso.IdMesa == mesaDestino.Id &&
                  uso.IdOrdenVenta != orden.Id &&
                  !ordenActual.Eliminado &&
                  ordenActual.Estado == (short)EstadoOrdenVenta.Abierta
            select uso.Id)
            .AnyAsync(cancellationToken);

        if (mesaDestinoOcupada)
        {
            throw new InvalidOperationException("La mesa de destino ya tiene una orden abierta.");
        }

        usoMesa.IdMesa = mesaDestino.Id;
        _usoMesaRepository.Update(usoMesa);
        await _usoMesaRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        var detalles = await _detalleRepository.Query()
            .Where(detalle => !detalle.Eliminado && detalle.IdOrdenVenta == orden.Id)
            .ToListAsync(cancellationToken);

        return new Response<OrdenMesaDTO>(OrdenMesaUtils.Mapear(orden, usoMesa, detalles));
    }

    private static void ValidarSolicitud(TransferirOrdenMesaDTO transferencia)
    {
        if (transferencia.IdOrdenVenta <= 0)
        {
            throw new InvalidOperationException("Debe seleccionar una orden de mesa válida.");
        }

        if (transferencia.IdMesaDestino <= 0)
        {
            throw new InvalidOperationException("Debe seleccionar una mesa de destino.");
        }
    }
}
