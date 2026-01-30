using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Domain.Interfaces.Shared;
using MediatR;

namespace Application.Features.Inventory.Almacenes.Commands;

public class CreateAlmacenCommand : ICommand<Response<long>>
{
    public required AlmacenDTO AlmacenDTO { get; set; }
}

public class CreateAlmacenHandler : ICommandHandler<CreateAlmacenCommand, Response<long>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IRepository<Almacen> _repository;

    public CreateAlmacenHandler(IMediator mediator, IMapper mapper, IRepository<Almacen> repository)
    {
        _mediator = mediator;
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<long>> Handle(CreateAlmacenCommand request, CancellationToken cancellationToken)
    {
        Almacen almacen = _mapper.Map<Almacen>(request.AlmacenDTO);
        almacen = await _repository.AddAsync(almacen);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<long>(almacen.Id);
    }
}
