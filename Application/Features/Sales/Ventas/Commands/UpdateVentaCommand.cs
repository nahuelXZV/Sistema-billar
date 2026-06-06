using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sales.Ventas.Commands;

public class UpdateVentaCommand : ICommand<Response<bool>>
{
    public required VentaDTO VentaDTO { get; set; }
}

public class UpdateVentaCommandHandler : ICommandHandler<UpdateVentaCommand, Response<bool>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Venta> _repository;
    private readonly IRepository<VentaDetalle> _detalleRepository;
    private readonly IRepository<PagoVenta> _pagoRepository;

    public UpdateVentaCommandHandler(
        IMapper mapper,
        IRepository<Venta> repository,
        IRepository<VentaDetalle> detalleRepository,
        IRepository<PagoVenta> pagoRepository)
    {
        _mapper = mapper;
        _repository = repository;
        _detalleRepository = detalleRepository;
        _pagoRepository = pagoRepository;
    }

    public async Task<Response<bool>> Handle(UpdateVentaCommand request, CancellationToken cancellationToken)
    {
        var venta = await _repository.GetByIdAsync(request.VentaDTO.Id);
        if (venta == null) throw new ArgumentException("La venta no existe.");

        _mapper.Map(request.VentaDTO, venta);
        if (venta.IdOrdenVenta == 0) venta.IdOrdenVenta = null;
        _repository.Update(venta);

        var detallesExistentes = await _detalleRepository.Query()
            .Where(d => d.IdVenta == venta.Id)
            .ToListAsync(cancellationToken);
        var pagosExistentes = await _pagoRepository.Query()
            .Where(p => p.IdVenta == venta.Id)
            .ToListAsync(cancellationToken);

        if (detallesExistentes.Count > 0)
            _detalleRepository.DeleteRange(detallesExistentes, false);
        if (pagosExistentes.Count > 0)
            _pagoRepository.DeleteRange(pagosExistentes, false);

        var nuevosDetalles = _mapper.Map<List<VentaDetalle>>(
            request.VentaDTO.ListaDetalles ?? []);
        foreach (var detalle in nuevosDetalles)
        {
            detalle.Id = 0;
            detalle.IdVenta = venta.Id;
            if (detalle.IdOrdenVentaDetalle == 0)
                detalle.IdOrdenVentaDetalle = null;
        }

        var nuevosPagos = _mapper.Map<List<PagoVenta>>(
            request.VentaDTO.ListaPagos ?? []);
        foreach (var pago in nuevosPagos)
        {
            pago.Id = 0;
            pago.IdVenta = venta.Id;
        }

        if (nuevosDetalles.Count > 0)
            await _detalleRepository.AddRangeAsync(nuevosDetalles);
        if (nuevosPagos.Count > 0)
            await _pagoRepository.AddRangeAsync(nuevosPagos);

        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}
