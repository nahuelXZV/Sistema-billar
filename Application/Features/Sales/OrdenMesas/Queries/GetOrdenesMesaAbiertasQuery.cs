using Application.Interfaces;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using static Domain.Constants.Constantes;
using Application.Common.Utils;

namespace Application.Features.Sales.OrdenMesas.Queries;

public class GetOrdenesMesaAbiertasQuery : IQuery<Response<List<OrdenMesaDTO>>>
{
}

public class GetOrdenesMesaAbiertasQueryHandler : IQueryHandler<GetOrdenesMesaAbiertasQuery, Response<List<OrdenMesaDTO>>>
{
    private readonly IRepository<OrdenVenta> _ordenRepository;
    private readonly IRepository<OrdenVentaDetalle> _detalleRepository;
    private readonly IRepository<UsoMesa> _usoMesaRepository;

    public GetOrdenesMesaAbiertasQueryHandler(IRepository<OrdenVenta> ordenRepository, IRepository<OrdenVentaDetalle> detalleRepository, IRepository<UsoMesa> usoMesaRepository)
    {
        _ordenRepository = ordenRepository;
        _detalleRepository = detalleRepository;
        _usoMesaRepository = usoMesaRepository;
    }

    public async Task<Response<List<OrdenMesaDTO>>> Handle(GetOrdenesMesaAbiertasQuery request, CancellationToken tokenCancelacion)
    {
        var usos = await _usoMesaRepository.Query().Where(uso => !uso.Eliminado && uso.Estado != (short)EstadoUsoMesa.Finalizado).ToListAsync(tokenCancelacion);

        if (usos.Count == 0) return new Response<List<OrdenMesaDTO>>([]);

        var idsOrdenes = usos.Select(uso => uso.IdOrdenVenta).Distinct().ToList();
        var ordenes = await _ordenRepository.Query().Where(orden => !orden.Eliminado && orden.Estado == (short)EstadoOrdenVenta.Abierta && idsOrdenes.Contains(orden.Id))
            .ToListAsync(tokenCancelacion);

        var detalles = await _detalleRepository.Query()
            .Where(detalle => !detalle.Eliminado && idsOrdenes.Contains(detalle.IdOrdenVenta))
            .ToListAsync(tokenCancelacion);

        var respuesta = (from uso in usos
                         join orden in ordenes on uso.IdOrdenVenta equals orden.Id
                         select OrdenMesaUtils.Mapear(orden, uso, detalles.Where(detalle => detalle.IdOrdenVenta == orden.Id))).ToList();

        return new Response<List<OrdenMesaDTO>>(respuesta);
    }
}
