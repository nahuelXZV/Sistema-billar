using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using MediatR;

namespace Application.Features.Inventory.Almacenes.Commands;

public class DeleteAlmacenCommand : ICommand<Response<bool>>
{
    public long AlmacenId { get; set; }
}

public class DeleteAlmacenHandler : ICommandHandler<DeleteAlmacenCommand, Response<bool>>
{
    private readonly IRepository<Almacen> _repository;

    public DeleteAlmacenHandler(IRepository<Almacen> repository)
    {
        _repository = repository;
    }

    public async Task<Response<bool>> Handle(DeleteAlmacenCommand request, CancellationToken cancellationToken)
    {
        var almacen = await _repository.GetByIdAsync(request.AlmacenId);
        if (almacen == null) throw new ArgumentException("El almacén no existe.");

        _repository.Update(almacen);
        almacen.Eliminado = true;

        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}

