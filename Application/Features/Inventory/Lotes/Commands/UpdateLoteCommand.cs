using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using MediatR;

namespace Application.Features.Inventory.Lotes.Commands;

public class UpdateLoteCommand : ICommand<Response<bool>>
{
    public LoteDTO LoteDTO { get; set; }
}

public class UpdateLoteHandler : ICommandHandler<UpdateLoteCommand, Response<bool>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IRepository<Lote> _repository;

    public UpdateLoteHandler(IMediator mediator, IMapper mapper, IRepository<Lote> repository)
    {
        _mediator = mediator;
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<bool>> Handle(UpdateLoteCommand request, CancellationToken cancellationToken)
    {
        var lote = await _repository.GetByIdAsync(request.LoteDTO.Id);
        if (lote == null) throw new ArgumentException("El lote no existe.");

        _repository.Attach(lote);
        _mapper.Map(request.LoteDTO, lote);

        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}
