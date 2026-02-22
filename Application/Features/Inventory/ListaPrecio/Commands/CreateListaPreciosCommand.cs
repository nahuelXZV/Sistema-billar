using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using MediatR;

namespace Application.Features.Inventory.ListaPrecio.Commands;

public class CreateListaPreciosCommand : ICommand<Response<long>>
{
    public required ListaPrecioDTO ListaPrecioDTO { get; set; }
}

public class CreateListaPreciosHandler : ICommandHandler<CreateListaPreciosCommand, Response<long>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IRepository<ListaPrecios> _repository;
    private readonly IRepository<ListaPreciosDetalle> _rpDetalles;

    public CreateListaPreciosHandler(IMediator mediator, IMapper mapper, IRepository<ListaPrecios> repository, IRepository<ListaPreciosDetalle> rpDetalles)
    {
        _mediator = mediator;
        _mapper = mapper;
        _repository = repository;
        _rpDetalles = rpDetalles;
    }

    public async Task<Response<long>> Handle(CreateListaPreciosCommand request, CancellationToken cancellationToken)
    {
        ListaPrecios transaccion = _mapper.Map<ListaPrecios>(request.ListaPrecioDTO);
        transaccion = await _repository.AddAsync(transaccion);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<long>(transaccion.Id);
    }
}
