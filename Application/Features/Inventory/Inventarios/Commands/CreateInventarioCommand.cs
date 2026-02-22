using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using MediatR;

namespace Application.Features.Inventory.Inventarios.Commands;

public class CreateInventarioCommand : ICommand<Response<long>>
{
    public required InventarioDTO InventarioDTO { get; set; }
}

public class CreateInventarioHandler : ICommandHandler<CreateInventarioCommand, Response<long>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IRepository<Inventario> _repository;

    public CreateInventarioHandler(IMediator mediator, IMapper mapper, IRepository<Inventario> repository)
    {
        _mediator = mediator;
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<long>> Handle(CreateInventarioCommand request, CancellationToken cancellationToken)
    {
        Inventario inventario = _mapper.Map<Inventario>(request.InventarioDTO);
        inventario.FechaActualizacion = DateTime.Now;
        inventario = await _repository.AddAsync(inventario);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return new Response<long>(inventario.Id);
    }
}
