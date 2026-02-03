using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using MediatR;

namespace Application.Features.Inventory.UnidadesMedidas.Commands;

public class UpdateUnidadMedidaCommand : ICommand<Response<bool>>
{
    public UnidadMedidaDTO UnidadMedidaDTO { get; set; }
}

public class UpdateUnidadMedidaCommandHandler : ICommandHandler<UpdateUnidadMedidaCommand, Response<bool>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IRepository<UnidadMedida> _repository;

    public UpdateUnidadMedidaCommandHandler(IMediator mediator, IMapper mapper, IRepository<UnidadMedida> repository)
    {
        _mediator = mediator;
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<bool>> Handle(UpdateUnidadMedidaCommand request, CancellationToken cancellationToken)
    {
        var unidad = await _repository.GetByIdAsync(request.UnidadMedidaDTO.Id);
        if (unidad == null) throw new ArgumentException("La unidad de medida no existe.");

        _repository.Update(unidad);
        _mapper.Map(request.UnidadMedidaDTO, unidad);

        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}
