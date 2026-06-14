using Application.Common.Utils;
using Application.Interfaces;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using static Domain.Constants.Constantes;

namespace Application.Features.Sales.OrdenVentas.Commands;

public class UpdateOrdenVentaCommand : ICommand<Response<bool>>
{
    public required OrdenMesaDTO OrdenMesa { get; set; }
}

public class UpdateOrdenVentaCommandHandler : ICommandHandler<UpdateOrdenVentaCommand, Response<bool>>
{
    private readonly IRepository<OrdenVenta> _ordenRepository;
    private readonly IRepository<OrdenVentaDetalle> _detalleRepository;
    private readonly IRepository<UsoMesa> _usoMesaRepository;

    public UpdateOrdenVentaCommandHandler(IRepository<OrdenVenta> repository, IRepository<OrdenVentaDetalle> detalleRepository,
        IRepository<UsoMesa> usoMesaRepository)
    {
        _ordenRepository = repository;
        _detalleRepository = detalleRepository;
        _usoMesaRepository = usoMesaRepository;
    }

    public async Task<Response<bool>> Handle(UpdateOrdenVentaCommand request, CancellationToken cancellationToken)
    {
        var orden = await _ordenRepository.Query().FirstOrDefaultAsync(ordenActual => !ordenActual.Eliminado && ordenActual.Id == request.OrdenMesa.Id,
                         cancellationToken) ?? throw new InvalidOperationException("La orden de mesa no existe.");

        if (orden.Estado != (short)EstadoOrdenVenta.Abierta) throw new InvalidOperationException("La orden de mesa ya está cerrada.");

        var usoMesa = await _usoMesaRepository.Query()
             .FirstOrDefaultAsync(uso => !uso.Eliminado && uso.IdOrdenVenta == orden.Id && uso.IdMesa == request.OrdenMesa.IdMesa,
                 cancellationToken) ?? throw new InvalidOperationException("El uso de la mesa no existe.");

        orden.IdCliente = request.OrdenMesa.IdCliente;
        orden.Observacion = request.OrdenMesa.Observacion;
        OrdenMesaUtils.CalcularTotales(orden, request.OrdenMesa);
        _ordenRepository.Update(orden);

        usoMesa.TarifaAplicada = request.OrdenMesa.TarifaAplicada;
        OrdenMesaUtils.ActualizarTiempo(usoMesa, DateTime.Now);
        _usoMesaRepository.Update(usoMesa);

        var detallesAnteriores = await _detalleRepository.Query()
            .Where(detalle => !detalle.Eliminado && detalle.IdOrdenVenta == orden.Id)
            .ToListAsync(cancellationToken);

        if (detallesAnteriores.Count > 0) _detalleRepository.DeleteRange(detallesAnteriores);

        var detalles = OrdenMesaUtils.CrearDetalles(request.OrdenMesa, orden.Id, usoMesa.Id);
        if (detalles.Count > 0) await _detalleRepository.AddRangeAsync(detalles);

        await _ordenRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}
