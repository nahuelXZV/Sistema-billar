using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using MediatR;

namespace Application.Features.Inventory.Inventarios.Commands;

public class UpdateInventarioCommand : ICommand<Response<bool>>
{
    public InventarioDTO InventarioDTO { get; set; }
}

public class UpdateInventarioHandler : ICommandHandler<UpdateInventarioCommand, Response<bool>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IRepository<Inventario> _repository;

    public UpdateInventarioHandler(IMediator mediator, IMapper mapper, IRepository<Inventario> repository)
    {
        _mediator = mediator;
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<bool>> Handle(UpdateInventarioCommand request, CancellationToken cancellationToken)
    {
        var inventario = await _repository.GetByIdAsync(request.InventarioDTO.Id);
        if (inventario == null) throw new ArgumentException("El inventario no existe.");

        inventario.FechaActualizacion = DateTime.Now;
        _repository.Attach(inventario);
        _mapper.Map(request.InventarioDTO, inventario);

        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}


