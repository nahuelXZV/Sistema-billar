using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using MediatR;

namespace Application.Features.Inventory.Lotes.Commands;

public class CreateLoteCommand : ICommand<Response<long>>
{
    public required LoteDTO LoteDTO { get; set; }
}

public class CreateLoteHandler : ICommandHandler<CreateLoteCommand, Response<long>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IRepository<Lote> _repository;

    public CreateLoteHandler(IMediator mediator, IMapper mapper, IRepository<Lote> repository)
    {
        _mediator = mediator;
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<long>> Handle(CreateLoteCommand request, CancellationToken cancellationToken)
    {
        Lote lote = _mapper.Map<Lote>(request.LoteDTO);
        lote = await _repository.AddAsync(lote);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return new Response<long>(lote.Id);
    }
}
