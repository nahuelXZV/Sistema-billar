using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Configuration;
using Domain.Entities.Configuration;
using Infraestructure.Interfaces;
using MediatR;

namespace Application.Features.Configuration.Mesas.Commands;

public class UpdateMesaCommand : ICommand<Response<bool>>
{
    public required MesaDTO MesaDto { get; set; }
}

public class UpdateMesaCommandHandler : ICommandHandler<UpdateMesaCommand, Response<bool>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IRepository<Mesa> _repository;

    public UpdateMesaCommandHandler(IMediator mediator, IMapper mapper, IRepository<Mesa> repository)
    {
        _mediator = mediator;
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<bool>> Handle(UpdateMesaCommand request, CancellationToken cancellationToken)
    {
        var mesa = await _repository.GetByIdAsync(request.MesaDto.Id);
        if (mesa == null) throw new ArgumentException("La mesa no existe.");

        _repository.Update(mesa);
        _mapper.Map(request.MesaDto, mesa);

        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}
