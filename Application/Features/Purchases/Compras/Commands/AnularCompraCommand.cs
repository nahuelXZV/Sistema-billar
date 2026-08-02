using Application.Features.Inventory.TransaccionInventarios.Command;
using Application.Interfaces;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Domain.Entities.Purchases;
using Domain.Entities.Security;
using Infraestructure.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static Domain.Constants.Constantes;

namespace Application.Features.Purchases.Compras.Commands;

public class AnularCompraCommand : ICommand<Response<bool>>
{
    public long IdCompra { get; set; }
    public long IdUsuarioAnulacion { get; set; }
    public string Motivo { get; set; } = string.Empty;
}

public class AnularCompraCommandHandler : ICommandHandler<AnularCompraCommand, Response<bool>>
{
    private readonly IMediator _mediator;
    private readonly IRepository<Compra> _compraRepository;
    private readonly IRepository<Usuario> _usuarioRepository;
    private readonly IRepository<TransaccionInventarioDetalle> _transaccionDetalleRepository;

    public AnularCompraCommandHandler(
        IMediator mediator,
        IRepository<Compra> compraRepository,
        IRepository<Usuario> usuarioRepository,
        IRepository<TransaccionInventarioDetalle> transaccionDetalleRepository)
    {
        _mediator = mediator;
        _compraRepository = compraRepository;
        _usuarioRepository = usuarioRepository;
        _transaccionDetalleRepository = transaccionDetalleRepository;
    }

    public async Task<Response<bool>> Handle(AnularCompraCommand request, CancellationToken cancellationToken)
    {
        if (request.IdCompra <= 0)
        {
            throw new ArgumentException("La compra seleccionada no es válida.");
        }

        if (string.IsNullOrWhiteSpace(request.Motivo) || request.Motivo.Trim().Length > 500)
        {
            throw new ArgumentException("Debe indicar un motivo de anulación de hasta 500 caracteres.");
        }

        var usuarioValido = await _usuarioRepository.Query()
            .AnyAsync(usuario =>
                usuario.Id == request.IdUsuarioAnulacion &&
                !usuario.Eliminado &&
                usuario.Activo,
                cancellationToken);

        if (!usuarioValido)
        {
            throw new InvalidOperationException("El usuario que anula la compra no existe o está inactivo.");
        }

        var compra = await _compraRepository.Query()
            .FirstOrDefaultAsync(item => item.Id == request.IdCompra && !item.Eliminado, cancellationToken)
            ?? throw new ArgumentException("La compra no existe.");

        if (compra.Estado == (short)EstadoCompra.Anulada)
        {
            throw new InvalidOperationException("La compra ya fue anulada.");
        }

        if (!compra.IdTransaccionInventario.HasValue)
        {
            throw new InvalidOperationException("La compra no tiene un movimiento de inventario asociado.");
        }

        var detallesMovimiento = await _transaccionDetalleRepository.Query()
            .Where(detalle =>
                detalle.IdTransaccion == compra.IdTransaccionInventario.Value &&
                !detalle.Eliminado)
            .ToListAsync(cancellationToken);

        if (detallesMovimiento.Count == 0)
        {
            throw new InvalidOperationException("El movimiento de inventario de la compra no tiene detalles para revertir.");
        }

        await _mediator.Send(new CreateTransaccionInventarioCommand
        {
            TransaccionInventarioDTO = new TransaccionInventarioDTO
            {
                Tipo = (short)TipoTransaccionInventario.Salida,
                Fecha = DateTime.Now,
                Glosa = $"Anulación de compra {compra.Numero}",
                IdUsuario = request.IdUsuarioAnulacion,
                IdTransaccionInicial = compra.Id,
                Detalles = detallesMovimiento.Select(detalle => new TransaccionInventarioDetalleDTO
                {
                    IdProducto = detalle.IdProducto,
                    IdAlmacen = detalle.IdAlmacen,
                    IdLote = detalle.IdLote,
                    Cantidad = detalle.Cantidad
                }).ToList()
            }
        }, cancellationToken);

        _compraRepository.Attach(compra);
        compra.Estado = (short)EstadoCompra.Anulada;
        compra.FechaAnulacion = DateTime.Now;
        compra.IdUsuarioAnulacion = request.IdUsuarioAnulacion;
        compra.MotivoAnulacion = request.Motivo.Trim();

        await _compraRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}
