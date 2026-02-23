using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Configuration;
using Domain.Entities.Configuration;
using Infraestructure.Interfaces;
using MediatR;

namespace Application.Features.Configuration.TipoMesas.Commands;

public class UpdateTipoMesaCommand : ICommand<Response<bool>>
{
    public TipoMesaDTO TipoMesaDto { get; set; }
}

public class UpdateTipoMesaCommandHandler : ICommandHandler<UpdateTipoMesaCommand, Response<bool>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IRepository<TipoMesa> _repository;

    public UpdateTipoMesaCommandHandler(IMediator mediator, IMapper mapper, IRepository<TipoMesa> repository)
    {
        _mediator = mediator;
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<bool>> Handle(UpdateTipoMesaCommand request, CancellationToken cancellationToken)
    {
        var tipoMesa = await _repository.GetByIdAsync(request.TipoMesaDto.Id);
        if (tipoMesa == null) throw new ArgumentException("El Tipo de mesa no existe.");

        _repository.Update(tipoMesa);
        _mapper.Map(request.TipoMesaDto, tipoMesa);

        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}


