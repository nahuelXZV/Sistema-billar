using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.Entities.Configuration;
using Infraestructure.Interfaces;
using MediatR;

namespace Application.Features.Configuration.TipoMesas.Commands;

public class DeleteTipoMesaCommand : ICommand<Response<bool>>
{
    public long Id { get; set; }
}

public class DeleteTipoMesaCommandHandler : ICommandHandler<DeleteTipoMesaCommand, Response<bool>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IRepository<TipoMesa> _repository;

    public DeleteTipoMesaCommandHandler(IMediator mediator, IMapper mapper, IRepository<TipoMesa> repository)
    {
        _mediator = mediator;
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<bool>> Handle(DeleteTipoMesaCommand request, CancellationToken cancellationToken)
    {
        var tipoMesa = await _repository.GetByIdAsync(request.Id);
        if (tipoMesa == null) throw new ArgumentException("El Tipo de mesa no existe.");

        _repository.Attach(tipoMesa);
        tipoMesa.Eliminado = true;
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}


