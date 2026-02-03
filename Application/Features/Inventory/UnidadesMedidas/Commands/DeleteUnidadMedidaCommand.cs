using Application.Interfaces;
using Domain.Common;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;

namespace Application.Features.Inventory.UnidadesMedidas.Commands;

public class DeleteUnidadMedidaCommand : ICommand<Response<bool>>
{
    public long UnidadId { get; set; }
}

public class DeleteUnidadMedidaCommandHandler : ICommandHandler<DeleteUnidadMedidaCommand, Response<bool>>
{
    private readonly IRepository<UnidadMedida> _repository;

    public DeleteUnidadMedidaCommandHandler(IRepository<UnidadMedida> repository)
    {
        _repository = repository;
    }

    public async Task<Response<bool>> Handle(DeleteUnidadMedidaCommand request, CancellationToken cancellationToken)
    {
        var unidad = await _repository.GetByIdAsync(request.UnidadId);
        if (unidad == null) throw new ArgumentException("La unidad no existe.");

        _repository.Update(unidad);
        unidad.Eliminado = true;

        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}
