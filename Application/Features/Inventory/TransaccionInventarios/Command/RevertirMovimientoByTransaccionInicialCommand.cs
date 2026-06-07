using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static Domain.Constants.Constantes;

namespace Application.Features.Inventory.TransaccionInventarios.Command;

public class RevertirMovimientoByTransaccionInicialCommand : ICommand<Response<long>>
{
    public required long IdTransaccionInicial { get; set; }
    public required long IdUsuario { get; set; }
}

public class RevertirMovimientoByTransaccionInicialCommandHandler : ICommandHandler<RevertirMovimientoByTransaccionInicialCommand, Response<long>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IRepository<TransaccionInventario> _repository;
    private readonly IRepository<TransaccionInventarioDetalle> _rpDetalles;

    public RevertirMovimientoByTransaccionInicialCommandHandler(IMediator mediator, IMapper mapper, IRepository<TransaccionInventario> repository, IRepository<TransaccionInventarioDetalle> rpDetalles)
    {
        _mediator = mediator;
        _mapper = mapper;
        _repository = repository;
        _rpDetalles = rpDetalles;
    }

    public async Task<Response<long>> Handle(RevertirMovimientoByTransaccionInicialCommand request, CancellationToken cancellationToken)
    {
        var transaccion = await _repository.Query()
            .Where(m => m.IdTransaccionInicial == request.IdTransaccionInicial && !m.Eliminado)
            .FirstOrDefaultAsync(cancellationToken);

        if (transaccion == null)
            throw new Exception("Movimiento de inventario no encontrado");

        var detalles = await _rpDetalles.Query()
            .Where(d => d.IdTransaccion == transaccion.Id && !d.Eliminado)
            .ToListAsync(cancellationToken);

        await _mediator.Send(new CreateTransaccionInventarioCommand()
        {
            TransaccionInventarioDTO = new()
            {
                IdTransaccionInicial = request.IdTransaccionInicial,
                Glosa = $"Revertido - {transaccion.Glosa}",
                Fecha = DateTime.Now,
                IdUsuario = request.IdUsuario,
                Tipo = ObtenerTipo(transaccion.Tipo),
                Detalles = detalles.Select(d => new TransaccionInventarioDetalleDTO()
                {
                    IdAlmacen = d.IdAlmacen,
                    IdProducto = d.IdProducto,
                    IdLote = d.IdLote,
                    Cantidad = (double)d.Cantidad,
                }).ToList() ?? []
            }
        }, cancellationToken);

        return new Response<long>(transaccion.Id);
    }

    private short ObtenerTipo(short tipo)
    {
        if (tipo == (short)TipoTransaccionInventario.Salida || tipo == (short)TipoTransaccionInventario.Merma)
            return (short)TipoTransaccionInventario.Ingreso;

        return (short)TipoTransaccionInventario.Salida;
    }
}
