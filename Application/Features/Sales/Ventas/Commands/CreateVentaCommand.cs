using Application.Helpers;
using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static Domain.Constants.Constantes;

namespace Application.Features.Sales.Ventas.Commands;

public class CreateVentaCommand : ICommand<Response<long>>
{
    public required VentaDTO VentaDTO { get; set; }
}

public class CreateVentaCommandHandler : ICommandHandler<CreateVentaCommand, Response<long>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Venta> _ventaRepository;
    private readonly IRepository<TurnoCaja> _turnoCajaRepository;
    private readonly IDbContext _dbContext;
    private readonly IMediator _mediator;

    public CreateVentaCommandHandler(
        IMapper mapper,
        IRepository<Venta> ventaRepository,
        IRepository<TurnoCaja> turnoCajaRepository,
        IDbContext dbContext,
        IMediator mediator)
    {
        _mapper = mapper;
        _ventaRepository = ventaRepository;
        _turnoCajaRepository = turnoCajaRepository;
        _dbContext = dbContext;
        _mediator = mediator;
    }

    public async Task<Response<long>> Handle(CreateVentaCommand solicitud, CancellationToken tokenCancelacion)
    {
        var claveIdempotencia = ObtenerClaveIdempotencia(solicitud.VentaDTO);
        var idVentaExistente = await ObtenerIdVentaPorIdempotenciaAsync(claveIdempotencia, tokenCancelacion);

        if (idVentaExistente > 0)
        {
            return new Response<long>(idVentaExistente);
        }

        await _mediator.Send(new ValidarVentaCommand { Venta = solicitud.VentaDTO }, tokenCancelacion);

        var venta = CrearEntidadVenta(solicitud.VentaDTO, claveIdempotencia);
        venta.IdTurnoCaja = await ObtenerIdTurnoCajaAbiertoAsync(venta.IdVendedor, tokenCancelacion);

        if (!venta.IdTurnoCaja.HasValue)
        {
            throw new InvalidOperationException("El vendedor debe tener un turno de caja abierto para registrar ventas.");
        }

        var resultadoGuardado = await GuardarVentaAsync(venta, tokenCancelacion);
        venta = resultadoGuardado.Venta;

        if (resultadoGuardado.YaExistia)
        {
            return new Response<long>(venta.Id);
        }

        await _mediator.Send(new CrearMovimientoInventarioVentaCommand
        {
            IdVenta = venta.Id,
            IdVendedor = venta.IdVendedor,
            DetallesVenta = solicitud.VentaDTO.ListaDetalles ?? []
        }, tokenCancelacion);

        if (venta.IdOrdenVenta.HasValue)
        {
            await _mediator.Send(new AplicarPagoOrdenMesaCommand
            {
                IdOrdenVenta = venta.IdOrdenVenta.Value,
                TotalVenta = venta.Total,
                FinalizarOrdenVenta = solicitud.VentaDTO.FinalizarOrdenVenta,
                DetallesPagados = solicitud.VentaDTO.ListaDetalles ?? []
            }, tokenCancelacion);
        }

        return new Response<long>(venta.Id);
    }

    private Venta CrearEntidadVenta(VentaDTO ventaDto, Guid claveIdempotencia)
    {
        var venta = _mapper.Map<Venta>(ventaDto);
        venta.Id = 0;
        venta.IdempotencyKey = claveIdempotencia;
        venta.Numero = string.Empty;
        venta.ListaDetalles = _mapper.Map<List<VentaDetalle>>(ventaDto.ListaDetalles ?? []);
        venta.ListaPagos = _mapper.Map<List<PagoVenta>>(ventaDto.ListaPagos ?? []);

        if (venta.IdOrdenVenta == 0)
        {
            venta.IdOrdenVenta = null;
        }

        foreach (var detalle in venta.ListaDetalles)
        {
            detalle.Id = 0;
            detalle.IdVenta = 0;

            if (detalle.IdOrdenVentaDetalle == 0)
            {
                detalle.IdOrdenVentaDetalle = null;
            }
        }

        foreach (var pago in venta.ListaPagos)
        {
            pago.Id = 0;
            pago.IdVenta = 0;
        }

        return venta;
    }

    private async Task<(Venta Venta, bool YaExistia)> GuardarVentaAsync(Venta venta, CancellationToken tokenCancelacion)
    {
        venta = await _ventaRepository.AddAsync(venta);

        try
        {
            await _ventaRepository.UnitOfWork.SaveEntitiesAsync(tokenCancelacion);
        }
        catch (DbUpdateException)
        {
            _dbContext.dbContext.ChangeTracker.Clear();
            var claveIdempotencia = venta.IdempotencyKey ?? throw new InvalidOperationException("La venta no tiene una clave de idempotencia.");

            var idVentaExistente = await ObtenerIdVentaPorIdempotenciaAsync(claveIdempotencia, tokenCancelacion);

            if (idVentaExistente > 0)
            {
                var ventaExistente = await _ventaRepository.Query().FirstAsync(item => item.Id == idVentaExistente, tokenCancelacion);
                return (ventaExistente, true);
            }

            throw;
        }

        //venta.Numero = $"V-{venta.Fecha:yyyyMMdd}-{venta.Id:D8}";
        venta.Numero = GenerarCodigoHelper.Generar("V", venta.Id);
        _ventaRepository.Update(venta);
        await _ventaRepository.UnitOfWork.SaveEntitiesAsync(tokenCancelacion);

        return (venta, false);
    }

    private static Guid ObtenerClaveIdempotencia(VentaDTO venta)
    {
        if (!venta.IdempotencyKey.HasValue || venta.IdempotencyKey == Guid.Empty)
        {
            throw new InvalidOperationException("La venta debe incluir una clave de idempotencia válida.");
        }

        return venta.IdempotencyKey.Value;
    }

    private async Task<long> ObtenerIdVentaPorIdempotenciaAsync(Guid claveIdempotencia, CancellationToken tokenCancelacion)
    {
        return await _ventaRepository.Query()
            .Where(venta => venta.IdempotencyKey == claveIdempotencia)
            .Select(venta => venta.Id)
            .FirstOrDefaultAsync(tokenCancelacion);
    }

    private async Task<long?> ObtenerIdTurnoCajaAbiertoAsync(long idVendedor, CancellationToken tokenCancelacion)
    {
        var idTurnoCaja = await _turnoCajaRepository.Query()
            .Where(turno =>
                turno.IdVendedor == idVendedor &&
                turno.Estado == (short)EstadoTurnoCaja.Abierto &&
                !turno.Eliminado)
            .Select(turno => turno.Id)
            .FirstOrDefaultAsync(tokenCancelacion);

        return idTurnoCaja > 0 ? idTurnoCaja : null;
    }
}
