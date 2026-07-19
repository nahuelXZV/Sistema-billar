using Application.Interfaces;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using static Domain.Constants.Constantes;

namespace Application.Features.Sales.TurnosCaja.Commands;

public class UpdateTurnoCajaCommand : ICommand<Response<bool>>
{
    public required TurnoCajaDTO TurnoCajaDTO { get; set; }
}

public class UpdateTurnoCajaCommandHandler : ICommandHandler<UpdateTurnoCajaCommand, Response<bool>>
{
    private readonly IRepository<TurnoCaja> _turnoCajaRepository;
    private readonly IRepository<TurnoCajaDetalle> _detalleRepository;
    private readonly IRepository<MetodoPago> _metodoPagoRepository;

    public UpdateTurnoCajaCommandHandler(
        IRepository<TurnoCaja> turnoCajaRepository,
        IRepository<TurnoCajaDetalle> detalleRepository,
        IRepository<MetodoPago> metodoPagoRepository)
    {
        _turnoCajaRepository = turnoCajaRepository;
        _detalleRepository = detalleRepository;
        _metodoPagoRepository = metodoPagoRepository;
    }

    public async Task<Response<bool>> Handle(UpdateTurnoCajaCommand request, CancellationToken cancellationToken)
    {
        var dto = request.TurnoCajaDTO;
        var detallesSolicitados = dto.Detalles ?? [];

        var turnoCaja = await _turnoCajaRepository.Query()
            .FirstOrDefaultAsync(turno => turno.Id == dto.Id && !turno.Eliminado, cancellationToken)
            ?? throw new ArgumentException("El turno de caja no existe.");

        if (turnoCaja.Estado == (short)EstadoTurnoCaja.Cerrado)
            throw new InvalidOperationException("No se puede editar un turno de caja cerrado.");

        ValidarSolicitud(dto, detallesSolicitados);
        await ValidarMetodosPagoAsync(detallesSolicitados, cancellationToken);

        var cerrarTurno = dto.Estado == (short)EstadoTurnoCaja.Cerrado;
        if (cerrarTurno && detallesSolicitados.Any(detalle => !detalle.MontoCierreDeclarado.HasValue))
            throw new InvalidOperationException("Debe registrar el monto de cierre de todos los métodos de pago.");

        var detallesGuardados = await _detalleRepository.Query()
            .Where(detalle => detalle.IdTurnoCaja == turnoCaja.Id && !detalle.Eliminado)
            .ToListAsync(cancellationToken);

        foreach (var detalleGuardado in detallesGuardados)
        {
            if (detallesSolicitados.All(detalle => detalle.IdMetodoPago != detalleGuardado.IdMetodoPago))
            {
                _detalleRepository.Delete(detalleGuardado);
            }
        }

        foreach (var detalleDto in detallesSolicitados)
        {
            var montoVentas = detalleDto.MontoVentasSistema ?? 0;
            decimal? diferencia = cerrarTurno ? detalleDto.MontoCierreDeclarado!.Value - (detalleDto.MontoApertura + montoVentas) : null;

            var detalleGuardado = detallesGuardados.FirstOrDefault(detalle => detalle.IdMetodoPago == detalleDto.IdMetodoPago);

            if (detalleGuardado is null)
            {
                await _detalleRepository.AddAsync(new TurnoCajaDetalle
                {
                    IdTurnoCaja = turnoCaja.Id,
                    IdMetodoPago = detalleDto.IdMetodoPago,
                    MontoApertura = detalleDto.MontoApertura,
                    MontoVentasSistema = montoVentas,
                    MontoCierreDeclarado = cerrarTurno ? detalleDto.MontoCierreDeclarado : null,
                    Diferencia = diferencia
                });
                continue;
            }

            _detalleRepository.Attach(detalleGuardado);
            detalleGuardado.MontoApertura = detalleDto.MontoApertura;
            detalleGuardado.MontoVentasSistema = montoVentas;
            detalleGuardado.MontoCierreDeclarado = cerrarTurno ? detalleDto.MontoCierreDeclarado : null;
            detalleGuardado.Diferencia = diferencia;
        }

        _turnoCajaRepository.Attach(turnoCaja);
        turnoCaja.Observacion = dto.Observacion.Trim();
        turnoCaja.Estado = cerrarTurno ? (short)EstadoTurnoCaja.Cerrado : (short)EstadoTurnoCaja.Abierto;
        turnoCaja.FechaCierre = cerrarTurno ? DateTime.Now : null;

        await _turnoCajaRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }

    private static void ValidarSolicitud(TurnoCajaDTO dto, IReadOnlyCollection<TurnoCajaDetalleDTO> detalles)
    {
        if (dto.Id <= 0)
            throw new InvalidOperationException("El identificador del turno de caja no es válido.");

        if (dto.Estado != (short)EstadoTurnoCaja.Abierto && dto.Estado != (short)EstadoTurnoCaja.Cerrado)
            throw new InvalidOperationException("El estado del turno de caja no es válido.");

        if (detalles.Count == 0)
            throw new InvalidOperationException("Debe registrar al menos un método de pago.");

        if (detalles.Any(detalle => detalle.IdMetodoPago <= 0))
            throw new InvalidOperationException("Todos los detalles deben tener un método de pago válido.");

        if (detalles.Any(detalle => detalle.MontoApertura < 0 || detalle.MontoVentasSistema < 0 || detalle.MontoCierreDeclarado < 0))
            throw new InvalidOperationException("Los montos del turno no pueden ser negativos.");

        if (detalles.Select(detalle => detalle.IdMetodoPago).Distinct().Count() != detalles.Count)
            throw new InvalidOperationException("No se puede repetir un método de pago en el turno.");
    }

    private async Task ValidarMetodosPagoAsync(IEnumerable<TurnoCajaDetalleDTO> detalles, CancellationToken cancellationToken)
    {
        var idsMetodosPago = detalles.Select(detalle => detalle.IdMetodoPago).Distinct().ToList();
        var cantidadMetodosValidos = await _metodoPagoRepository.Query().CountAsync(metodo =>
                idsMetodosPago.Contains(metodo.Id) &&
                metodo.Activo &&
                !metodo.Eliminado,
                cancellationToken);

        if (cantidadMetodosValidos != idsMetodosPago.Count)
            throw new InvalidOperationException("Uno o más métodos de pago no existen o están inactivos.");
    }
}
