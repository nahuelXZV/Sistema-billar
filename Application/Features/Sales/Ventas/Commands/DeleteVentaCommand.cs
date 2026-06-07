using Application.Features.Inventory.TransaccionInventarios.Command;
using Application.Interfaces;
using Domain.Common;
using Domain.Entities.Inventory;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static Domain.Constants.Constantes;

namespace Application.Features.Sales.Ventas.Commands;

public class DeleteVentaCommand : ICommand<Response<bool>>
{
    public long IdVenta { get; set; }
    public long IdUsuario { get; set; }
}

public class DeleteVentaCommandHandler : ICommandHandler<DeleteVentaCommand, Response<bool>>
{
    private readonly IRepository<Venta> _repository;
    private readonly IRepository<TransaccionInventario> _transaccionInventarioRepository;
    private readonly IMediator _mediator;

    public DeleteVentaCommandHandler(
        IRepository<Venta> repository,
        IRepository<TransaccionInventario> transaccionInventarioRepository,
        IMediator mediator)
    {
        _repository = repository;
        _transaccionInventarioRepository = transaccionInventarioRepository;
        _mediator = mediator;
    }

    public async Task<Response<bool>> Handle(DeleteVentaCommand request, CancellationToken cancellationToken)
    {
        var venta = await _repository.Query()
            .Where(v => v.Id == request.IdVenta && !v.Eliminado)
            .FirstOrDefaultAsync(cancellationToken);

        if (venta == null) throw new ArgumentException("La venta no existe.");

        var tieneMovimientoInventario = await _transaccionInventarioRepository.Query()
            .AnyAsync(
                movimiento =>
                    movimiento.IdTransaccionInicial == venta.Id &&
                    !movimiento.Eliminado &&
                    movimiento.Tipo == (short)TipoTransaccionInventario.Salida,
                cancellationToken);

        _repository.Delete(venta);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        if (tieneMovimientoInventario)
        {
            await _mediator.Send(new RevertirMovimientoByTransaccionInicialCommand
            {
                IdTransaccionInicial = venta.Id,
                IdUsuario = request.IdUsuario
            }, cancellationToken);
        }

        return new Response<bool>(true);
    }
}
