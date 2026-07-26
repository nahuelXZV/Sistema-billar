using Application.Common.Utils;
using Application.Features.Inventory.TransaccionInventarios.Command;
using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Configuration;
using Domain.Entities.Inventory;
using FluentValidation;
using Infraestructure.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static Domain.Constants.Constantes;

namespace Application.Features.Inventory.TraspasoInventarios.Commands;

public class CreateTraspasoInventarioCommand : ICommand<Response<long>>
{
    public required TraspasoInventarioDTO TraspasoInventarioDTO { get; set; }
}

public class CreateTraspasoInventarioHandler : ICommandHandler<CreateTraspasoInventarioCommand, Response<long>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IRepository<TraspasoInventario> _traspasoRepository;
    private readonly IRepository<TraspasoInventarioDetalle> _detalleRepository;
    private readonly IRepository<Almacen> _almacenRepository;
    private readonly IRepository<Producto> _productoRepository;
    private readonly IRepository<Lote> _loteRepository;
    private readonly IRepository<Inventario> _inventarioRepository;
    private readonly IValidator<TraspasoInventarioDTO> _validator;

    public CreateTraspasoInventarioHandler(
        IMediator mediator,
        IMapper mapper,
        IRepository<TraspasoInventario> traspasoRepository,
        IRepository<TraspasoInventarioDetalle> detalleRepository,
        IRepository<Almacen> almacenRepository,
        IRepository<Producto> productoRepository,
        IRepository<Lote> loteRepository,
        IRepository<Inventario> inventarioRepository,
        IValidator<TraspasoInventarioDTO> validator)
    {
        _mediator = mediator;
        _mapper = mapper;
        _traspasoRepository = traspasoRepository;
        _detalleRepository = detalleRepository;
        _almacenRepository = almacenRepository;
        _productoRepository = productoRepository;
        _loteRepository = loteRepository;
        _inventarioRepository = inventarioRepository;
        _validator = validator;
    }

    public async Task<Response<long>> Handle(CreateTraspasoInventarioCommand request, CancellationToken cancellationToken)
    {
        var traspasoDto = request.TraspasoInventarioDTO;
        await _validator.ValidateAndThrowAsync(traspasoDto, cancellationToken);

        var detalles = traspasoDto.Detalles
            .GroupBy(detalle => new
            {
                detalle.IdProducto,
                IdLote = detalle.IdLote > 0 ? detalle.IdLote : null
            })
            .Select(grupo => new TraspasoInventarioDetalleDTO
            {
                IdProducto = grupo.Key.IdProducto,
                IdLote = grupo.Key.IdLote,
                Cantidad = grupo.Sum(detalle => detalle.Cantidad)
            })
            .ToList();

        var idsAlmacenes = new[] { traspasoDto.IdAlmacenOrigen, traspasoDto.IdAlmacenDestino };
        var cantidadAlmacenes = await _almacenRepository.Query().CountAsync(almacen => idsAlmacenes.Contains(almacen.Id) && !almacen.Eliminado, cancellationToken);

        if (cantidadAlmacenes != idsAlmacenes.Length)
        {
            throw new InvalidOperationException("El almacén origen o destino no existe.");
        }

        var idsProductos = detalles.Select(detalle => detalle.IdProducto).Distinct().ToList();
        var productosValidos = await _productoRepository.Query()
            .Where(producto => idsProductos.Contains(producto.Id)
                && !producto.Eliminado
                && producto.Activo
                && producto.Tipo == (short)TipoProducto.Producto)
            .Select(producto => producto.Id)
            .ToListAsync(cancellationToken);

        var productosMesa = await _productoRepository.Query<TipoMesa>()
            .Where(tipoMesa => !tipoMesa.Eliminado
                && tipoMesa.IdProducto.HasValue
                && idsProductos.Contains(tipoMesa.IdProducto.Value))
            .Select(tipoMesa => tipoMesa.IdProducto!.Value)
            .ToListAsync(cancellationToken);

        if (productosValidos.Count != idsProductos.Count || productosMesa.Count > 0)
        {
            throw new InvalidOperationException("El traspaso contiene productos inexistentes, inactivos o configurados como mesa.");
        }

        var idsLotes = detalles
            .Where(detalle => detalle.IdLote.HasValue)
            .Select(detalle => detalle.IdLote!.Value)
            .Distinct()
            .ToList();

        if (idsLotes.Count > 0)
        {
            var lotes = await _loteRepository.Query()
                .Where(lote => idsLotes.Contains(lote.Id) && !lote.Eliminado && lote.Activo)
                .Select(lote => new { lote.Id, lote.IdProducto })
                .ToListAsync(cancellationToken);

            if (lotes.Count != idsLotes.Count || detalles.Any(detalle =>
                    detalle.IdLote.HasValue
                    && !lotes.Any(lote => lote.Id == detalle.IdLote.Value && lote.IdProducto == detalle.IdProducto)))
            {
                throw new InvalidOperationException("Uno o más lotes no existen, están inactivos o no corresponden al producto indicado.");
            }
        }

        var parametros = new ValidarStockDisponibleParametros
        {
            IdAlmacen = traspasoDto.IdAlmacenOrigen,
            Detalles = detalles.Select(detalle => (detalle.IdProducto, detalle.IdLote, detalle.Cantidad)),
            ContextoAlmacen = "origen"
        };
        await InventarioUtils.ValidarStockDisponibleAsync(_inventarioRepository, parametros, cancellationToken);

        var traspaso = _mapper.Map<TraspasoInventario>(traspasoDto);
        traspaso.Id = 0;
        traspaso.Estado = (short)EstadoTraspasoInventario.Confirmado;

        traspaso = await _traspasoRepository.AddAsync(traspaso);
        await _traspasoRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        var detallesEntidad = detalles.Select(detalle => new TraspasoInventarioDetalle
        {
            IdTraspasoInventario = traspaso.Id,
            IdProducto = detalle.IdProducto,
            IdLote = detalle.IdLote,
            Cantidad = detalle.Cantidad
        }).ToList();

        await _detalleRepository.AddRangeAsync(detallesEntidad);
        await _detalleRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        await _mediator.Send(new CreateTransaccionInventarioCommand
        {
            TransaccionInventarioDTO = new TransaccionInventarioDTO
            {
                Tipo = (short)TipoTransaccionInventario.Salida,
                Fecha = traspaso.Fecha,
                Glosa = $"Salida por traspaso: {traspaso.Glosa}",
                IdUsuario = traspaso.IdUsuario,
                IdTransaccionInicial = traspaso.Id,
                Detalles = detalles.Select(detalle => new TransaccionInventarioDetalleDTO
                {
                    IdProducto = detalle.IdProducto,
                    IdLote = detalle.IdLote,
                    IdAlmacen = traspaso.IdAlmacenOrigen,
                    Cantidad = (double)detalle.Cantidad
                }).ToList()
            }
        }, cancellationToken);

        await _mediator.Send(new CreateTransaccionInventarioCommand
        {
            TransaccionInventarioDTO = new TransaccionInventarioDTO
            {
                Tipo = (short)TipoTransaccionInventario.Ingreso,
                Fecha = traspaso.Fecha,
                Glosa = $"Ingreso por traspaso: {traspaso.Glosa}",
                IdUsuario = traspaso.IdUsuario,
                IdTransaccionInicial = traspaso.Id,
                Detalles = detalles.Select(detalle => new TransaccionInventarioDetalleDTO
                {
                    IdProducto = detalle.IdProducto,
                    IdLote = detalle.IdLote,
                    IdAlmacen = traspaso.IdAlmacenDestino,
                    Cantidad = (double)detalle.Cantidad
                }).ToList()
            }
        }, cancellationToken);

        return new Response<long>(traspaso.Id);
    }
}
