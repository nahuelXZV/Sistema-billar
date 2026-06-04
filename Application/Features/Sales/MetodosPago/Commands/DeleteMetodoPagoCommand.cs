using Application.Interfaces;
using Domain.Common;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;

namespace Application.Features.Sales.MetodosPago.Commands;

public class DeleteMetodoPagoCommand : ICommand<Response<bool>>
{
    public long MetodoPagoId { get; set; }
}

public class DeleteMetodoPagoCommandHandler : ICommandHandler<DeleteMetodoPagoCommand, Response<bool>>
{
    private readonly IRepository<MetodoPago> _repository;

    public DeleteMetodoPagoCommandHandler(IRepository<MetodoPago> repository)
    {
        _repository = repository;
    }

    public async Task<Response<bool>> Handle(DeleteMetodoPagoCommand request, CancellationToken cancellationToken)
    {
        var metodoPago = await _repository.GetByIdAsync(request.MetodoPagoId);
        if (metodoPago == null) throw new ArgumentException("El metodo de pago no existe.");

        _repository.Update(metodoPago);
        metodoPago.Eliminado = true;

        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}
