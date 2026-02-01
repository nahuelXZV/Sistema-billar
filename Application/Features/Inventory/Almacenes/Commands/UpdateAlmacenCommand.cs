using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using MediatR;

namespace Application.Features.Inventory.Almacenes.Commands;

public class UpdateAlmacenCommand : ICommand<Response<bool>>
{
    public AlmacenDTO AlmacenDTO { get; set; }
}

public class UpdateAlmacenHandler : ICommandHandler<UpdateAlmacenCommand, Response<bool>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IRepository<Almacen> _repository;

    public UpdateAlmacenHandler(IMediator mediator, IMapper mapper, IRepository<Almacen> repository)
    {
        _mediator = mediator;
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<bool>> Handle(UpdateAlmacenCommand request, CancellationToken cancellationToken)
    {
        var almacen = await _repository.GetByIdAsync(request.AlmacenDTO.Id);
        if (almacen == null) throw new ArgumentException("El almacén no existe.");

        _repository.Update(almacen);
        _mapper.Map(request.AlmacenDTO, almacen);

        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}

