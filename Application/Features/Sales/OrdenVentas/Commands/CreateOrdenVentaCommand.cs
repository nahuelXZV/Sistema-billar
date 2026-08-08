using Application.Common.Utils;
using Application.Helpers;
using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Domain.Utils;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using static Domain.Constants.Constantes;

namespace Application.Features.Sales.OrdenVentas.Commands;

public class CreateOrdenVentaCommand : ICommand<Response<long>>
{
    public required OrdenMesaDTO OrdenMesa { get; set; }
}

public class CreateOrdenVentaCommandHandler : ICommandHandler<CreateOrdenVentaCommand, Response<long>>
{
    private readonly IRepository<OrdenVenta> _ordenRepository;
    private readonly IRepository<OrdenVentaDetalle> _detalleRepository;
    private readonly IRepository<UsoMesa> _usoMesaRepository;

    public CreateOrdenVentaCommandHandler(IRepository<OrdenVenta> repository, IRepository<OrdenVentaDetalle> detalleRepository, IRepository<UsoMesa> usoMesaRepository)
    {
        _ordenRepository = repository;
        _detalleRepository = detalleRepository;
        _usoMesaRepository = usoMesaRepository;
    }

    public async Task<Response<long>> Handle(CreateOrdenVentaCommand request, CancellationToken cancellationToken)
    {
        var mesaOcupada = await (from uso in _usoMesaRepository.Query()
                                 join ordenActual in _ordenRepository.Query() on uso.IdOrdenVenta equals ordenActual.Id
                                 where !uso.Eliminado && uso.IdMesa == request.OrdenMesa.IdMesa &&
                                       !ordenActual.Eliminado && ordenActual.Estado == (short)EstadoOrdenVenta.Abierta
                                 select uso.Id).AnyAsync(cancellationToken);

        if (mesaOcupada) throw new InvalidOperationException("La mesa ya tiene una orden abierta.");

        var orden = OrdenMesaUtils.MapearOrden(request.OrdenMesa);
        orden = await _ordenRepository.AddAsync(orden);
        await _ordenRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        //orden.Numero = $"OV-{orden.FechaApertura:yyyyMMdd}-{orden.Id:D8}";
        orden.Numero = GenerarCodigoHelper.Generar("OV", orden.Id);

        _ordenRepository.Update(orden);

        var usoMesa = new UsoMesa
        {
            IdOrdenVenta = orden.Id,
            IdMesa = request.OrdenMesa.IdMesa,
            FechaInicio = DateTime.Now,
            MinutosConsumidos = 0,
            TarifaAplicada = request.OrdenMesa.TarifaAplicada,
            MontoCalculado = 0,
            Estado = (short)EstadoUsoMesa.Pendiente
        };

        usoMesa = await _usoMesaRepository.AddAsync(usoMesa);
        await _usoMesaRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        var detalles = OrdenMesaUtils.CrearDetalles(request.OrdenMesa, orden.Id, usoMesa.Id);
        if (detalles.Count > 0) await _detalleRepository.AddRangeAsync(detalles);

        await _ordenRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<long>(orden.Id);
    }

}
