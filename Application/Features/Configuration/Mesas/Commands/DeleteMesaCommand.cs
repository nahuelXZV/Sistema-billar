using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.Entities.Configuration;
using Infraestructure.Interfaces;
using MediatR;

namespace Application.Features.Configuration.Mesas.Commands;

public class DeleteMesaCommand : ICommand<Response<bool>>
{
    public long Id { get; set; }
}

public class DeleteMesaCommandHandler : ICommandHandler<DeleteMesaCommand, Response<bool>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IRepository<Mesa> _repository;

    public DeleteMesaCommandHandler(IMediator mediator, IMapper mapper, IRepository<Mesa> repository)
    {
        _mediator = mediator;
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<bool>> Handle(DeleteMesaCommand request, CancellationToken cancellationToken)
    {
        var mesa = await _repository.GetByIdAsync(request.Id);
        if (mesa == null) throw new ArgumentException("La mesa no existe.");

        _repository.Attach(mesa);
        mesa.Eliminado = true;
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}
