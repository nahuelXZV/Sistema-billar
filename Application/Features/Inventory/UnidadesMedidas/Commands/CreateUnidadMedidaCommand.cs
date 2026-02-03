using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using MediatR;

namespace Application.Features.Inventory.UnidadesMedidas.Commands;

public class CreateUnidadMedidaCommand : ICommand<Response<long>>
{
    public required UnidadMedidaDTO UnidadMedidaDTO { get; set; }
}

public class CreateUnidadMedidaCommandHandler : ICommandHandler<CreateUnidadMedidaCommand, Response<long>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IRepository<UnidadMedida> _repository;

    public CreateUnidadMedidaCommandHandler(IMediator mediator, IMapper mapper, IRepository<UnidadMedida> repository)
    {
        _mediator = mediator;
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<long>> Handle(CreateUnidadMedidaCommand request, CancellationToken cancellationToken)
    {
        UnidadMedida unidad = _mapper.Map<UnidadMedida>(request.UnidadMedidaDTO);
        unidad = await _repository.AddAsync(unidad);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken); 

        return new Response<long>(unidad.Id);
    }
}
