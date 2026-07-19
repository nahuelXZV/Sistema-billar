using Application.Interfaces;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Domain.Entities.Security;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using static Domain.Constants.Constantes;

namespace Application.Features.Sales.TurnosCaja.Commands;

public class CreateTurnoCajaCommand : ICommand<Response<long>>
{
    public required TurnoCajaDTO TurnoCajaDTO { get; set; }
    public long IdUsuario { get; set; }
}

public class CreateTurnoCajaCommandHandler : ICommandHandler<CreateTurnoCajaCommand, Response<long>>
{
    private readonly IRepository<TurnoCaja> _turnoCajaRepository;
    private readonly IRepository<Vendedor> _vendedorRepository;
    private readonly IRepository<MetodoPago> _metodoPagoRepository;
    private readonly IRepository<Usuario> _usuarioRepository;

    public CreateTurnoCajaCommandHandler(
        IRepository<TurnoCaja> turnoCajaRepository,
        IRepository<Vendedor> vendedorRepository,
        IRepository<MetodoPago> metodoPagoRepository,
        IRepository<Usuario> usuarioRepository)
    {
        _turnoCajaRepository = turnoCajaRepository;
        _vendedorRepository = vendedorRepository;
        _metodoPagoRepository = metodoPagoRepository;
        _usuarioRepository = usuarioRepository;
    }

    public async Task<Response<long>> Handle(CreateTurnoCajaCommand request, CancellationToken cancellationToken)
    {
        var dto = request.TurnoCajaDTO;
        var detalles = dto.Detalles ?? [];

        dto.IdVendedor = await ResolverIdVendedorAsync(
            request.IdUsuario,
            dto.IdVendedor,
            cancellationToken);

        ValidarSolicitud(dto, detalles);

        var vendedorExiste = await _vendedorRepository.Query().AnyAsync(vendedor =>
                vendedor.Id == dto.IdVendedor &&
                vendedor.Activo &&
                !vendedor.Eliminado,
                cancellationToken);

        if (!vendedorExiste)
            throw new InvalidOperationException("El vendedor no existe o está inactivo.");

        var tieneTurnoAbierto = await _turnoCajaRepository.Query().AnyAsync(turno =>
                turno.IdVendedor == dto.IdVendedor &&
                turno.Estado == (short)EstadoTurnoCaja.Abierto &&
                !turno.Eliminado,
                cancellationToken);

        if (tieneTurnoAbierto)
            throw new InvalidOperationException("El vendedor ya tiene un turno de caja abierto.");

        await ValidarMetodosPagoAsync(detalles, cancellationToken);

        var turnoCaja = new TurnoCaja
        {
            IdVendedor = dto.IdVendedor,
            FechaApertura = DateTime.Now,
            FechaCierre = null,
            Estado = (short)EstadoTurnoCaja.Abierto,
            Observacion = dto.Observacion.Trim(),
            Detalles = detalles.Select(detalle => new TurnoCajaDetalle
            {
                IdMetodoPago = detalle.IdMetodoPago,
                MontoApertura = detalle.MontoApertura,
                MontoVentasSistema = 0,
                MontoCierreDeclarado = null,
                Diferencia = null
            }).ToList()
        };

        turnoCaja = await _turnoCajaRepository.AddAsync(turnoCaja);
        await _turnoCajaRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<long>(turnoCaja.Id);
    }

    private async Task<long> ResolverIdVendedorAsync(
        long idUsuario,
        long idVendedorSolicitado,
        CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.Query()
            .Where(item => item.Id == idUsuario && item.Activo && !item.Eliminado)
            .Select(item => new
            {
                item.Perfil.EsSuperAdministrador,
                PerfilEliminado = item.Perfil.Eliminado
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new UnauthorizedAccessException("No se pudo identificar al usuario autenticado.");

        if (usuario.PerfilEliminado)
            throw new UnauthorizedAccessException("El perfil del usuario no está activo.");

        if (usuario.EsSuperAdministrador)
            return idVendedorSolicitado;

        var idVendedorUsuario = await _vendedorRepository.Query()
            .Where(vendedor =>
                vendedor.IdUsuario == idUsuario &&
                vendedor.Activo &&
                !vendedor.Eliminado)
            .Select(vendedor => vendedor.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (idVendedorUsuario <= 0)
            throw new InvalidOperationException("El usuario no tiene un vendedor activo asignado.");

        return idVendedorUsuario;
    }

    private static void ValidarSolicitud(TurnoCajaDTO dto, IReadOnlyCollection<TurnoCajaDetalleDTO> detalles)
    {
        if (dto.IdVendedor <= 0)
            throw new InvalidOperationException("Debe seleccionar un vendedor.");

        if (detalles.Count == 0)
            throw new InvalidOperationException("Debe registrar al menos un método de pago.");

        if (detalles.Any(detalle => detalle.IdMetodoPago <= 0))
            throw new InvalidOperationException("Todos los detalles deben tener un método de pago válido.");

        if (detalles.Any(detalle => detalle.MontoApertura < 0))
            throw new InvalidOperationException("Los montos de apertura no pueden ser negativos.");

        if (detalles.Select(detalle => detalle.IdMetodoPago).Distinct().Count() != detalles.Count)
            throw new InvalidOperationException("No se puede repetir un método de pago en el turno.");
    }

    private async Task ValidarMetodosPagoAsync(IEnumerable<TurnoCajaDetalleDTO> detalles, CancellationToken cancellationToken)
    {
        var idsMetodosPago = detalles.Select(detalle => detalle.IdMetodoPago).Distinct().ToList();
        var cantidadMetodosValidos = await _metodoPagoRepository.Query()
            .CountAsync(metodo =>
                idsMetodosPago.Contains(metodo.Id) &&
                metodo.Activo &&
                !metodo.Eliminado,
                cancellationToken);

        if (cantidadMetodosValidos != idsMetodosPago.Count)
            throw new InvalidOperationException("Uno o más métodos de pago no existen o están inactivos.");
    }
}
