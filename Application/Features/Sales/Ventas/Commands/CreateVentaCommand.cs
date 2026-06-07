using Application.Features.Inventory.TransaccionInventarios.Command;
using Application.Features.Sales.Vendedores.Queries;
using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using MediatR;
using static Domain.Constants.Constantes;

namespace Application.Features.Sales.Ventas.Commands;

public class CreateVentaCommand : ICommand<Response<long>>
{
    public required VentaDTO VentaDTO { get; set; }
}

public class CreateVentaCommandHandler : ICommandHandler<CreateVentaCommand, Response<long>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Venta> _repository;
    private readonly IMediator _mediator;

    public CreateVentaCommandHandler(IMapper mapper, IRepository<Venta> repository, IMediator mediator)
    {
        _mapper = mapper;
        _repository = repository;
        _mediator = mediator;
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


        // Crear Movimiento Productos
        var vendedor = (await _mediator.Send(new GetVendedorByIdQuery() { Id = request.VentaDTO.IdVendedor })).Data;
        await _mediator.Send(new CreateTransaccionInventarioCommand()
        {
            TransaccionInventarioDTO = new()
            {
                IdTransaccionInicial = venta.Id,
                Glosa = "Salida por venta",
                Fecha = DateTime.Now,
                IdUsuario = vendedor.IdUsuario,
                Tipo = (short)TipoTransaccionInventario.Salida,
                Detalles = request.VentaDTO.ListaDetalles?.Select(d => new TransaccionInventarioDetalleDTO()
                {
                    IdAlmacen = vendedor.ListaAlmacenes.FirstOrDefault()?.IdAlmacen ?? 0,
                    IdProducto = d.IdProducto,
                    Cantidad = (double)d.Cantidad,
                }).ToList() ?? []
            }
        });



        return new Response<long>(venta.Id);
    }
}
