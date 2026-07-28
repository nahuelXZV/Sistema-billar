using Application.Features.Inventory.Inventarios.Commands;
using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Configuration;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static Domain.Constants.Constantes;

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
        var detallesInventario = await ObtenerDetallesInventarioAsync(
            request.TransaccionInventarioDTO.Detalles,
            cancellationToken);

        if (detallesInventario.Count == 0)
        {
            throw new InvalidOperationException(
                "El movimiento no contiene productos físicos que afecten inventario.");
        }

        request.TransaccionInventarioDTO.Detalles = detallesInventario;

        TransaccionInventario transaccion = _mapper.Map<TransaccionInventario>(request.TransaccionInventarioDTO);
        transaccion = await _repository.AddAsync(transaccion);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        foreach (var detalleDto in detallesInventario)
        {
            var detalle = _mapper.Map<TransaccionInventarioDetalleDTO, TransaccionInventarioDetalle>(detalleDto);
            detalle.IdTransaccion = transaccion.Id;
            await _rpDetalles.AddAsync(detalle);
        }

        await _rpDetalles.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        await _mediator.Send(new UpdateStockCommand() { Transaccion = request.TransaccionInventarioDTO });

        return new Response<long>(transaccion.Id);
    }

    private async Task<List<TransaccionInventarioDetalleDTO>> ObtenerDetallesInventarioAsync(IEnumerable<TransaccionInventarioDetalleDTO> detalles, CancellationToken cancellationToken)
    {
        var listaDetalles = detalles.ToList();
        var idsProductos = listaDetalles
            .Select(d => d.IdProducto)
            .Distinct()
            .ToList();

        var productosFisicosIds = await _repository.Query<Producto>()
            .Where(p => idsProductos.Contains(p.Id)
                && !p.Eliminado
                && p.Tipo == (short)TipoProducto.Producto)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        var productosMesaIds = await _repository.Query<TipoMesa>()
            .Where(t => !t.Eliminado && t.IdProducto.HasValue && idsProductos.Contains(t.IdProducto.Value))
            .Select(t => t.IdProducto!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        var detallesFisicos = listaDetalles
            .Where(d => productosFisicosIds.Contains(d.IdProducto) && !productosMesaIds.Contains(d.IdProducto))
            .ToList();

        var idsConversiones = detallesFisicos
            .Where(detalle => detalle.IdProductoConversion.HasValue)
            .Select(detalle => detalle.IdProductoConversion!.Value)
            .Distinct()
            .ToList();

        var conversiones = idsConversiones.Count == 0
            ? []
            : await _repository.Query<ProductoConversion>()
                .AsNoTracking()
                .Include(conversion => conversion.UnidadMedida)
                .Where(conversion => !conversion.Eliminado && idsConversiones.Contains(conversion.Id))
                .ToListAsync(cancellationToken);

        foreach (var detalle in detallesFisicos)
        {
            if (detalle.Cantidad <= 0)
            {
                throw new InvalidOperationException($"La cantidad del producto {detalle.IdProducto} debe ser mayor a cero.");
            }

            if (!detalle.IdProductoConversion.HasValue)
            {
                detalle.FactorConversion = 1;
                continue;
            }

            var conversion = conversiones.FirstOrDefault(item =>
                item.Id == detalle.IdProductoConversion.Value)
                ?? throw new InvalidOperationException($"La unidad seleccionada para el producto {detalle.IdProducto} no existe.");

            if (conversion.IdProducto != detalle.IdProducto || conversion.FactorConversion <= 0)
            {
                throw new InvalidOperationException($"La unidad seleccionada no corresponde al producto {detalle.IdProducto}.");
            }

            detalle.NombreUnidadMedida = conversion.UnidadMedida?.Nombre ?? string.Empty;
            detalle.AbreviaturaUnidadMedida = conversion.UnidadMedida?.Abreviatura ?? string.Empty;
            detalle.FactorConversion = conversion.FactorConversion;
            detalle.Cantidad *= (double)conversion.FactorConversion;
        }

        return detallesFisicos;
    }
}
