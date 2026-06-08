using Application.Interfaces;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using static Domain.Constants.Constantes;
using Application.Helpers;

namespace Application.Features.Sales.OrdenMesas.Queries;

public class GetOrdenMesaByMesaQuery : ICommand<Response<OrdenMesaDTO?>>
{
    public long IdMesa { get; set; }
}

public class GetOrdenMesaByMesaQueryHandler : ICommandHandler<GetOrdenMesaByMesaQuery, Response<OrdenMesaDTO?>>
{
    private readonly IRepository<OrdenVenta> _ordenRepository;
    private readonly IRepository<OrdenVentaDetalle> _detalleRepository;
    private readonly IRepository<UsoMesa> _usoMesaRepository;

    public GetOrdenMesaByMesaQueryHandler(
        IRepository<OrdenVenta> ordenRepository,
        IRepository<OrdenVentaDetalle> detalleRepository,
        IRepository<UsoMesa> usoMesaRepository)
    {
        _ordenRepository = ordenRepository;
        _detalleRepository = detalleRepository;
        _usoMesaRepository = usoMesaRepository;
    }

    public async Task<Response<OrdenMesaDTO?>> Handle(GetOrdenMesaByMesaQuery request, CancellationToken tokenCancelacion)
    {
        var usoMesa = await (
            from uso in _usoMesaRepository.Query()
            join ordenActual in _ordenRepository.Query()
                on uso.IdOrdenVenta equals ordenActual.Id
            where !uso.Eliminado &&
                  uso.IdMesa == request.IdMesa &&
                  !ordenActual.Eliminado &&
                  ordenActual.Estado == (short)EstadoOrdenVenta.Abierta
            select uso)
            .FirstOrDefaultAsync(tokenCancelacion);

        if (usoMesa is null) return CrearRespuestaVacia();

        var orden = await _ordenRepository.Query()
            .FirstOrDefaultAsync(
                ordenActual => !ordenActual.Eliminado &&
                               ordenActual.Id == usoMesa.IdOrdenVenta &&
                               ordenActual.Estado == (short)EstadoOrdenVenta.Abierta,
                tokenCancelacion);

        if (orden is null) return CrearRespuestaVacia();

        var detalles = await _detalleRepository.Query()
            .Where(detalle => !detalle.Eliminado && detalle.IdOrdenVenta == orden.Id)
            .ToListAsync(tokenCancelacion);

        return new Response<OrdenMesaDTO?>(OrdenMesaMapeo.Crear(orden, usoMesa, detalles));
    }

    private static Response<OrdenMesaDTO?> CrearRespuestaVacia() => new()
    {
        Succeded = true,
        Data = null
    };
}
