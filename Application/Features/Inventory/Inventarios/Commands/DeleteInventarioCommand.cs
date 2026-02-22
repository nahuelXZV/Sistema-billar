using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using MediatR;

namespace Application.Features.Inventory.Inventarios.Commands;

public class DeleteInventarioCommand : ICommand<Response<bool>>
{
    public long Id { get; set; }
}

public class DeleteInventarioHandler : ICommandHandler<DeleteInventarioCommand, Response<bool>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IRepository<Inventario> _repository;

    public DeleteInventarioHandler(IMediator mediator, IMapper mapper, IRepository<Inventario> repository)
    {
        _mediator = mediator;
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<bool>> Handle(DeleteInventarioCommand request, CancellationToken cancellationToken)
    {
        var inventario = await _repository.GetByIdAsync(request.Id);
        if (inventario == null) throw new ArgumentException("El inventario no existe.");

        _repository.Attach(inventario);
        
        inventario.Eliminado = true;
        inventario.FechaActualizacion = DateTime.Now;

        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}

