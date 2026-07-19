using Application.Interfaces;
using Domain.Common;
using Domain.Entities.Sales;
using Domain.Entities.Security;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using static Domain.Constants.Constantes;

namespace Application.Features.Sales.TurnosCaja.Commands;

public class DeleteTurnoCajaCommand : ICommand<Response<bool>>
{
    public long Id { get; set; }
    public long IdUsuario { get; set; }
}

public class DeleteTurnoCajaCommandHandler : ICommandHandler<DeleteTurnoCajaCommand, Response<bool>>
{
    private readonly IRepository<TurnoCaja> _turnoCajaRepository;
    private readonly IRepository<TurnoCajaDetalle> _detalleRepository;
    private readonly IRepository<Usuario> _usuarioRepository;

    public DeleteTurnoCajaCommandHandler(
        IRepository<TurnoCaja> turnoCajaRepository,
        IRepository<TurnoCajaDetalle> detalleRepository,
        IRepository<Usuario> usuarioRepository)
    {
        _turnoCajaRepository = turnoCajaRepository;
        _detalleRepository = detalleRepository;
        _usuarioRepository = usuarioRepository;
    }

    public async Task<Response<bool>> Handle(DeleteTurnoCajaCommand request, CancellationToken cancellationToken)
    {
        var esSuperAdministrador = await _usuarioRepository.Query()
            .AnyAsync(usuario =>
                usuario.Id == request.IdUsuario &&
                usuario.Activo &&
                !usuario.Eliminado &&
                usuario.Perfil.EsSuperAdministrador &&
                !usuario.Perfil.Eliminado,
                cancellationToken);

        if (!esSuperAdministrador)
            throw new UnauthorizedAccessException("Solo un superadministrador puede eliminar turnos de caja.");

        var turnoCaja = await _turnoCajaRepository.Query()
            .FirstOrDefaultAsync(turno => turno.Id == request.Id && !turno.Eliminado, cancellationToken)
            ?? throw new ArgumentException("El turno de caja no existe.");

        if (turnoCaja.Estado == (short)EstadoTurnoCaja.Cerrado)
            throw new InvalidOperationException("No se puede eliminar un turno de caja cerrado.");

        var detalles = await _detalleRepository.Query()
            .Where(detalle => detalle.IdTurnoCaja == turnoCaja.Id && !detalle.Eliminado)
            .ToListAsync(cancellationToken);

        if (detalles.Count > 0) _detalleRepository.DeleteRange(detalles);

        _turnoCajaRepository.Delete(turnoCaja);
        await _turnoCajaRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}
