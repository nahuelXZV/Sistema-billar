using Application.Features.Inventory.TransaccionInventarios.Command;
using Application.Interfaces;
using Domain.Common;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using MediatR;

namespace Application.Features.Sales.Ventas.Commands;

public class DeleteVentaCommand : ICommand<Response<bool>>
{
    public long IdVenta { get; set; }
    public long IdUsuario { get; set; }
}

public class DeleteVentaCommandHandler : ICommandHandler<DeleteVentaCommand, Response<bool>>
{
    private readonly IRepository<Venta> _repository;
    private readonly IRepository<VentaDetalle> _rpDetalle;
    private readonly IMediator _mediator;

    public DeleteVentaCommandHandler(IRepository<Venta> repository, IRepository<VentaDetalle> rpDetalle, IMediator mediator)
    {
        _repository = repository;
        _mediator = mediator;
        _rpDetalle = rpDetalle;
    }

    public async Task<Response<bool>> Handle(DeleteVentaCommand request, CancellationToken cancellationToken)
    {
        var venta = await _repository.GetByIdAsync(request.IdVenta);
        if (venta == null) throw new ArgumentException("La venta no existe.");

        _repository.Delete(venta);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        await _mediator.Send(new RevertirMovimientoByTransaccionInicialCommand()
        {
            IdTransaccionInicial = venta.Id,
            IdUsuario = request.IdUsuario
        });

        return new Response<bool>(true);
    }
}
