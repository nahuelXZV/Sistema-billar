using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static Domain.Constants.Constantes;

namespace Application.Features.Inventory.Inventarios.Commands;

public class UpdateStockCommand : ICommand<Response<bool>>
{
    public TransaccionInventarioDTO Transaccion { get; set; }
}

public class UpdateStockHandler : ICommandHandler<UpdateStockCommand, Response<bool>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IRepository<Inventario> _repository;

    public UpdateStockHandler(IMediator mediator, IMapper mapper, IRepository<Inventario> repository)
    {
        _mediator = mediator;
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<bool>> Handle(UpdateStockCommand request, CancellationToken cancellationToken)
    {
        foreach (var detalle in request.Transaccion.Detalles)
        {
            var inventario = await _repository.Query()
                .Where(i => i.IdProducto == detalle.IdProducto)
                .Where(i => i.IdAlmacen == detalle.IdAlmacen)
                .Where(i => i.IdLote == detalle.IdLote)
                .FirstOrDefaultAsync(cancellationToken);

            if (inventario == null)
            {
                if (request.Transaccion.Tipo != (short)TipoTransaccionInventario.Ingreso)
                {
                    throw new InvalidOperationException(
                        $"No existe inventario para el producto {detalle.IdProducto} " +
                        $"en el almacén {detalle.IdAlmacen}.");
                }

                var inventarioNuevo = new Inventario()
                {
                    IdAlmacen = detalle.IdAlmacen,
                    IdLote = detalle.IdLote,
                    IdProducto = detalle.IdProducto,
                    FechaActualizacion = DateTime.Now,
                    Cantidad = detalle.Cantidad,
                    Reservado = 0,
                };
                await _repository.AddAsync(inventarioNuevo);
                continue;
            }

            _repository.Attach(inventario);
            if (request.Transaccion.Tipo == (short)TipoTransaccionInventario.Ingreso)
            {
                inventario.Cantidad += detalle.Cantidad;
            }
            else if (request.Transaccion.Tipo == (short)TipoTransaccionInventario.Salida || request.Transaccion.Tipo == (short)TipoTransaccionInventario.Merma)
            {
                if (inventario.Cantidad < detalle.Cantidad)
                {
                    throw new InvalidOperationException(
                        $"Stock insuficiente para el producto {detalle.IdProducto} " +
                        $"en el almacén {detalle.IdAlmacen}. " +
                        $"Disponible: {inventario.Cantidad}; solicitado: {detalle.Cantidad}.");
                }
                inventario.Cantidad -= detalle.Cantidad;
            }

        }
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}


