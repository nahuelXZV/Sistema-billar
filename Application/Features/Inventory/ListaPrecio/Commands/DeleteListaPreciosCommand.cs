using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using MediatR;
namespace Application.Features.Inventory.ListaPrecio.Commands;

public class DeleteListaPreciosCommand : ICommand<Response<bool>>
{
    public required long Id { get; set; }
}

public class DeleteListaPreciosHandler : ICommandHandler<DeleteListaPreciosCommand, Response<bool>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IRepository<ListaPrecios> _repository;

    public DeleteListaPreciosHandler(IMediator mediator, IMapper mapper, IRepository<ListaPrecios> repository)
    {
        _mediator = mediator;
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<bool>> Handle(DeleteListaPreciosCommand request, CancellationToken cancellationToken)
    {
        var listaPrecio = await _repository.GetByIdAsync(request.Id);
        if (listaPrecio == null) throw new ArgumentException("La lista de precios no existe.");

        _repository.Attach(listaPrecio);
        listaPrecio.Eliminado = true;
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}