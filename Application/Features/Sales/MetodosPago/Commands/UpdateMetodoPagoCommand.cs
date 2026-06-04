using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using MediatR;

namespace Application.Features.Sales.MetodosPago.Commands;

public class UpdateMetodoPagoCommand : ICommand<Response<bool>>
{
    public MetodoPagoDTO MetodoPagoDTO { get; set; }
}

public class UpdateMetodoPagoCommandHandler : ICommandHandler<UpdateMetodoPagoCommand, Response<bool>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IRepository<MetodoPago> _repository;

    public UpdateMetodoPagoCommandHandler(IMediator mediator, IMapper mapper, IRepository<MetodoPago> repository)
    {
        _mediator = mediator;
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<bool>> Handle(UpdateMetodoPagoCommand request, CancellationToken cancellationToken)
    {
        var metodoPago = await _repository.GetByIdAsync(request.MetodoPagoDTO.Id);
        if (metodoPago == null) throw new ArgumentException("El metodo de pago no existe.");

        _repository.Update(metodoPago);
        _mapper.Map(request.MetodoPagoDTO, metodoPago);

        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}
