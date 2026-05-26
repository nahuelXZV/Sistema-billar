using Application.Interfaces;
using Domain.Common;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;

namespace Application.Features.Sales.UsoMesas.Commands;

public class DeleteUsoMesaCommand : ICommand<Response<bool>>
{
    public long UsoMesaId { get; set; }
}

public class DeleteUsoMesaCommandHandler : ICommandHandler<DeleteUsoMesaCommand, Response<bool>>
{
    private readonly IRepository<UsoMesa> _repository;

    public DeleteUsoMesaCommandHandler(IRepository<UsoMesa> repository)
    {
        _repository = repository;
    }

    public async Task<Response<bool>> Handle(DeleteUsoMesaCommand request, CancellationToken cancellationToken)
    {
        var usoMesa = await _repository.GetByIdAsync(request.UsoMesaId);
        if (usoMesa == null) throw new ArgumentException("El uso de mesa no existe.");

        _repository.Delete(usoMesa);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}
