using Application.Features.Configuration.TipoMesas.Queries;
using Application.Features.Inventory.Inventarios.Commands;
using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using MediatR;

namespace Application.Features.Inventory.TransaccionInventarios.Command;

public class CreateTransaccionInventarioCommand : ICommand<Response<long>>
{
    public required TransaccionInventarioDTO TransaccionInventarioDTO { get; set; }
}

public class CreateTransaccionInventarioHandler : ICommandHandler<CreateTransaccionInventarioCommand, Response<long>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IRepository<TransaccionInventario> _repository;
    private readonly IRepository<TransaccionInventarioDetalle> _rpDetalles;

    public CreateTransaccionInventarioHandler(IMediator mediator, IMapper mapper, IRepository<TransaccionInventario> repository, IRepository<TransaccionInventarioDetalle> rpDetalles)
    {
        _mediator = mediator;
        _mapper = mapper;
        _repository = repository;
        _rpDetalles = rpDetalles;
    }

    public async Task<Response<long>> Handle(CreateTransaccionInventarioCommand request, CancellationToken cancellationToken)
    {
        TransaccionInventario transaccion = _mapper.Map<TransaccionInventario>(request.TransaccionInventarioDTO);
        transaccion = await _repository.AddAsync(transaccion);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        foreach (var detalles in request.TransaccionInventarioDTO.Detalles)
        {
            var esMesa = (await _mediator.Send(new VerificarMesaByIdProductoQuery() { IdProducto = detalles.IdProducto }, cancellationToken)).Data;
            if (esMesa) continue;

            var detalle = _mapper.Map<TransaccionInventarioDetalleDTO, TransaccionInventarioDetalle>(detalles);
            detalle.IdTransaccion = transaccion.Id;
            await _rpDetalles.AddAsync(detalle);
        }

        await _rpDetalles.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        await _mediator.Send(new UpdateStockCommand() { Transaccion = request.TransaccionInventarioDTO });

        return new Response<long>(transaccion.Id);
    }
}
