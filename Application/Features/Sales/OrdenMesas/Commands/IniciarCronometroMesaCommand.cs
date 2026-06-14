using Application.Interfaces;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using static Domain.Constants.Constantes;
using Application.Common.Utils;

namespace Application.Features.Sales.OrdenMesas.Commands;

public class IniciarCronometroMesaCommand : ICommand<Response<OrdenMesaDTO>>
{
    public long IdOrdenVenta { get; set; }
}

public class IniciarCronometroMesaCommandHandler : ICommandHandler<IniciarCronometroMesaCommand, Response<OrdenMesaDTO>>
{
    private readonly IRepository<OrdenVenta> _ordenRepository;
    private readonly IRepository<OrdenVentaDetalle> _detalleRepository;
    private readonly IRepository<UsoMesa> _usoMesaRepository;

    public IniciarCronometroMesaCommandHandler(IRepository<OrdenVenta> ordenRepository, IRepository<OrdenVentaDetalle> detalleRepository, IRepository<UsoMesa> usoMesaRepository)
    {
        _ordenRepository = ordenRepository;
        _detalleRepository = detalleRepository;
        _usoMesaRepository = usoMesaRepository;
    }

    public async Task<Response<OrdenMesaDTO>> Handle(IniciarCronometroMesaCommand request, CancellationToken tokenCancelacion)
    {
        var orden = await _ordenRepository.Query().FirstOrDefaultAsync(ordenActual => !ordenActual.Eliminado && ordenActual.Id == request.IdOrdenVenta, tokenCancelacion)
            ?? throw new InvalidOperationException("La orden de mesa no existe.");

        if (orden.Estado != (short)EstadoOrdenVenta.Abierta) throw new InvalidOperationException("La orden de mesa ya está cerrada.");

        var usoMesa = await _usoMesaRepository.Query()
            .FirstOrDefaultAsync(uso => !uso.Eliminado && uso.IdOrdenVenta == orden.Id && uso.Estado != (short)EstadoUsoMesa.Finalizado, tokenCancelacion)
            ?? throw new InvalidOperationException("El uso activo de la mesa no existe.");

        if (usoMesa.Estado == (short)EstadoUsoMesa.Pendiente)
        {
            usoMesa.FechaInicio = DateTime.Now;
            usoMesa.MinutosConsumidos = 0;
            usoMesa.MontoCalculado = 0;
            usoMesa.Estado = (short)EstadoUsoMesa.EnCurso;
            _usoMesaRepository.Update(usoMesa);
            await _usoMesaRepository.UnitOfWork.SaveEntitiesAsync(tokenCancelacion);
        }

        var detalles = await _detalleRepository.Query().Where(detalle => !detalle.Eliminado && detalle.IdOrdenVenta == orden.Id)
            .ToListAsync(tokenCancelacion);

        var ordenMesaResponse = OrdenMesaUtils.Mapear(orden, usoMesa, detalles);
        return new Response<OrdenMesaDTO>(ordenMesaResponse);
    }
}
