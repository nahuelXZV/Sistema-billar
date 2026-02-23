using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Configuration;
using Domain.Entities.Configuration;
using Infraestructure.Interfaces;
using MediatR;

namespace Application.Features.Configuration.TipoMesas.Commands;

public class CreateTipoMesaCommand : ICommand<Response<long>>
{
    public required TipoMesaDTO TipoMesaDto { get; set; }
}

public class CreateTipoMesaCommandHandler : ICommandHandler<CreateTipoMesaCommand, Response<long>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IRepository<TipoMesa> _repository;

    public CreateTipoMesaCommandHandler(IMediator mediator, IMapper mapper, IRepository<TipoMesa> repository)
    {
        _mediator = mediator;
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<long>> Handle(CreateTipoMesaCommand request, CancellationToken cancellationToken)
    {
        TipoMesa tipoMesa = _mapper.Map<TipoMesa>(request.TipoMesaDto);
        tipoMesa = await _repository.AddAsync(tipoMesa);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<long>(tipoMesa.Id);
    }
}
