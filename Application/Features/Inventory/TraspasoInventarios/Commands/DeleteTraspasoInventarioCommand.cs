using Application.Common.Utils;
using Application.Features.Inventory.TransaccionInventarios.Command;
using Application.Interfaces;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static Domain.Constants.Constantes;

namespace Application.Features.Inventory.TraspasoInventarios.Commands;

public class DeleteTraspasoInventarioCommand : ICommand<Response<bool>>
{
    public required long Id { get; set; }
    public required long IdUsuario { get; set; }
}

public class DeleteTraspasoInventarioHandler : ICommandHandler<DeleteTraspasoInventarioCommand, Response<bool>>
{
    private readonly IMediator _mediator;
    private readonly IRepository<TraspasoInventario> _traspasoRepository;
    private readonly IRepository<TraspasoInventarioDetalle> _detalleRepository;
    private readonly IRepository<Inventario> _inventarioRepository;

    public DeleteTraspasoInventarioHandler(
        IMediator mediator,
        IRepository<TraspasoInventario> traspasoRepository,
        IRepository<TraspasoInventarioDetalle> detalleRepository,
        IRepository<Inventario> inventarioRepository)
    {
        _mediator = mediator;
        _traspasoRepository = traspasoRepository;
        _detalleRepository = detalleRepository;
        _inventarioRepository = inventarioRepository;
    }

    public async Task<Response<bool>> Handle(DeleteTraspasoInventarioCommand request, CancellationToken cancellationToken)
    {
        var traspaso = await _traspasoRepository.Query()
            .FirstOrDefaultAsync(item => item.Id == request.Id && !item.Eliminado, cancellationToken)
            ?? throw new InvalidOperationException("El traspaso de inventario no existe o ya fue eliminado.");

        var detalles = await _detalleRepository.Query()
            .Where(item => item.IdTraspasoInventario == traspaso.Id && !item.Eliminado)
            .ToListAsync(cancellationToken);

        if (detalles.Count == 0)
        {
            throw new InvalidOperationException("El traspaso no contiene detalles para revertir.");
        }

        await InventarioUtils.ValidarStockDisponibleAsync(
            _inventarioRepository,
            new ValidarStockDisponibleParametros
            {
                IdAlmacen = traspaso.IdAlmacenDestino,
                Detalles = detalles.Select(detalle => (detalle.IdProducto, detalle.IdLote, detalle.Cantidad)),
                ContextoAlmacen = "destino para revertir el traspaso"
            },
            cancellationToken);

        await _mediator.Send(new CreateTransaccionInventarioCommand
        {
            TransaccionInventarioDTO = new TransaccionInventarioDTO
            {
                Tipo = (short)TipoTransaccionInventario.Salida,
                Fecha = DateTime.Now,
                Glosa = $"Reversión de traspaso - salida: {traspaso.Glosa}",
                IdUsuario = request.IdUsuario,
                IdTransaccionInicial = traspaso.Id,
                Detalles = detalles.Select(detalle => new TransaccionInventarioDetalleDTO
                {
                    IdProducto = detalle.IdProducto,
                    IdLote = detalle.IdLote,
                    IdAlmacen = traspaso.IdAlmacenDestino,
                    Cantidad = (double)detalle.Cantidad
                }).ToList()
            }
        }, cancellationToken);

        await _mediator.Send(new CreateTransaccionInventarioCommand
        {
            TransaccionInventarioDTO = new TransaccionInventarioDTO
            {
                Tipo = (short)TipoTransaccionInventario.Ingreso,
                Fecha = DateTime.Now,
                Glosa = $"Reversión de traspaso - ingreso: {traspaso.Glosa}",
                IdUsuario = request.IdUsuario,
                IdTransaccionInicial = traspaso.Id,
                Detalles = detalles.Select(detalle => new TransaccionInventarioDetalleDTO
                {
                    IdProducto = detalle.IdProducto,
                    IdLote = detalle.IdLote,
                    IdAlmacen = traspaso.IdAlmacenOrigen,
                    Cantidad = (double)detalle.Cantidad
                }).ToList()
            }
        }, cancellationToken);

        _traspasoRepository.Attach(traspaso);
        traspaso.Estado = (short)EstadoTraspasoInventario.Revertido;
        traspaso.Eliminado = true;
        _traspasoRepository.Update(traspaso);

        foreach (var detalle in detalles)
        {
            _detalleRepository.Attach(detalle);
            detalle.Eliminado = true;
            _detalleRepository.Update(detalle);
        }

        await _traspasoRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}
