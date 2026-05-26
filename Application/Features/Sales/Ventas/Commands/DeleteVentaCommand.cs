using Application.Interfaces;
using Domain.Common;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;

namespace Application.Features.Sales.Ventas.Commands;

public class DeleteVentaCommand : ICommand<Response<bool>>
{
    public long VentaId { get; set; }
}

public class DeleteVentaCommandHandler : ICommandHandler<DeleteVentaCommand, Response<bool>>
{
    private readonly IRepository<Venta> _repository;

    public DeleteVentaCommandHandler(IRepository<Venta> repository)
    {
        _repository = repository;
    }

    public async Task<Response<bool>> Handle(DeleteVentaCommand request, CancellationToken cancellationToken)
    {
        var venta = await _repository.GetByIdAsync(request.VentaId);
        if (venta == null) throw new ArgumentException("La venta no existe.");

        _repository.Delete(venta);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}
