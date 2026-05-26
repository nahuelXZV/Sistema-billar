using Application.Interfaces;
using Domain.Common;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;

namespace Application.Features.Sales.OrdenVentas.Commands;

public class DeleteOrdenVentaCommand : ICommand<Response<bool>>
{
    public long OrdenVentaId { get; set; }
}

public class DeleteOrdenVentaCommandHandler : ICommandHandler<DeleteOrdenVentaCommand, Response<bool>>
{
    private readonly IRepository<OrdenVenta> _repository;

    public DeleteOrdenVentaCommandHandler(IRepository<OrdenVenta> repository)
    {
        _repository = repository;
    }

    public async Task<Response<bool>> Handle(DeleteOrdenVentaCommand request, CancellationToken cancellationToken)
    {
        var ordenVenta = await _repository.GetByIdAsync(request.OrdenVentaId);
        if (ordenVenta == null) throw new ArgumentException("La orden de venta no existe.");

        _repository.Delete(ordenVenta);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}
