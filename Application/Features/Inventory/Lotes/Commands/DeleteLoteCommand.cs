using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using MediatR;

namespace Application.Features.Inventory.Lotes.Commands;

public class DeleteLoteCommand : ICommand<Response<bool>>
{
    public long Id { get; set; }
}

public class DeleteLoteHandler : ICommandHandler<DeleteLoteCommand, Response<bool>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IRepository<Lote> _repository;

    public DeleteLoteHandler(IMediator mediator, IMapper mapper, IRepository<Lote> repository)
    {
        _mediator = mediator;
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<bool>> Handle(DeleteLoteCommand request, CancellationToken cancellationToken)
    {
        var lote = await _repository.GetByIdAsync(request.Id);
        if (lote == null) throw new ArgumentException("El lote no existe.");

        _repository.Attach(lote);
        lote.Eliminado = true;

        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}
