using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using MediatR;

namespace Application.Features.Sales.MetodosPago.Commands;

public class CreateMetodoPagoCommand : ICommand<Response<long>>
{
    public required MetodoPagoDTO MetodoPagoDTO { get; set; }
}

public class CreateMetodoPagoCommandHandler : ICommandHandler<CreateMetodoPagoCommand, Response<long>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IRepository<MetodoPago> _repository;

    public CreateMetodoPagoCommandHandler(IMediator mediator, IMapper mapper, IRepository<MetodoPago> repository)
    {
        _mediator = mediator;
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<long>> Handle(CreateMetodoPagoCommand request, CancellationToken cancellationToken)
    {
        var metodoPago = _mapper.Map<MetodoPago>(request.MetodoPagoDTO);
        metodoPago = await _repository.AddAsync(metodoPago);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<long>(metodoPago.Id);
    }
}
