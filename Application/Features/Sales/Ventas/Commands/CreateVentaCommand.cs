using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;

namespace Application.Features.Sales.Ventas.Commands;

public class CreateVentaCommand : ICommand<Response<long>>
{
    public required VentaDTO VentaDTO { get; set; }
}

public class CreateVentaCommandHandler : ICommandHandler<CreateVentaCommand, Response<long>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Venta> _repository;

    public CreateVentaCommandHandler(IMapper mapper, IRepository<Venta> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<long>> Handle(CreateVentaCommand request, CancellationToken cancellationToken)
    {
        Venta venta = _mapper.Map<Venta>(request.VentaDTO);
        venta.Id = 0;
        venta.ListaDetalles = _mapper.Map<List<VentaDetalle>>(
            request.VentaDTO.ListaDetalles ?? []);
        venta.ListaPagos = _mapper.Map<List<PagoVenta>>(
            request.VentaDTO.ListaPagos ?? []);

        if (venta.IdOrdenVenta == 0) venta.IdOrdenVenta = null;
        foreach (var detalle in venta.ListaDetalles)
        {
            detalle.Id = 0;
            detalle.IdVenta = 0;
            if (detalle.IdOrdenVentaDetalle == 0)
                detalle.IdOrdenVentaDetalle = null;
        }
        foreach (var pago in venta.ListaPagos)
        {
            pago.Id = 0;
            pago.IdVenta = 0;
        }

        venta = await _repository.AddAsync(venta);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<long>(venta.Id);
    }
}
