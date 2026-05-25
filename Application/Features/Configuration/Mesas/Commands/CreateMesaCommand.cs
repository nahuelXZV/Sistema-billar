using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Configuration;
using Domain.Entities.Configuration;
using Infraestructure.Interfaces;
using MediatR;

namespace Application.Features.Configuration.Mesas.Commands;

public class CreateMesaCommand : ICommand<Response<long>>
{
    public required MesaDTO MesaDto { get; set; }
}

public class CreateMesaCommandHandler : ICommandHandler<CreateMesaCommand, Response<long>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IRepository<Mesa> _repository;

    public CreateMesaCommandHandler(IMediator mediator, IMapper mapper, IRepository<Mesa> repository)
    {
        _mediator = mediator;
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<long>> Handle(CreateMesaCommand request, CancellationToken cancellationToken)
    {
        Mesa mesa = _mapper.Map<Mesa>(request.MesaDto);
        mesa = await _repository.AddAsync(mesa);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<long>(mesa.Id);
    }
}
