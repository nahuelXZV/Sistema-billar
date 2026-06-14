using Application.Common.Utils;
using Application.Features.Sales.OrdenVentas.Commands;
using Application.Interfaces;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Inventory;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sales.OrdenMesas.Commands;

public class GuardarOrdenMesaCommand : ICommand<Response<OrdenMesaDTO>>
{
    public required OrdenMesaDTO OrdenMesa { get; set; }
}

public class GuardarOrdenMesaCommandHandler : ICommandHandler<GuardarOrdenMesaCommand, Response<OrdenMesaDTO>>
{
    private readonly IMediator _mediator;
    private readonly IRepository<OrdenVenta> _ordenRepository;
    private readonly IRepository<UsoMesa> _usoMesaRepository;
    private readonly IRepository<Producto> _productoRepository;

    public GuardarOrdenMesaCommandHandler(
        IMediator mediator,
        IRepository<OrdenVenta> ordenRepository,
        IRepository<UsoMesa> usoMesaRepository,
        IRepository<Producto> productoRepository)
    {
        _mediator = mediator;
        _ordenRepository = ordenRepository;
        _usoMesaRepository = usoMesaRepository;
        _productoRepository = productoRepository;
    }

    public async Task<Response<OrdenMesaDTO>> Handle(GuardarOrdenMesaCommand request, CancellationToken tokenCancelacion)
    {
        var ordenId = request.OrdenMesa.Id;

        ValidarSolicitud(request.OrdenMesa);
        await ValidarProductos(request.OrdenMesa.Detalles, tokenCancelacion);

        if (ordenId == 0)
        {
            ordenId = (await _mediator.Send(new CreateOrdenVentaCommand() { OrdenMesa = request.OrdenMesa })).Data;
        }
        else
        {
            var respuesta = await _mediator.Send(new UpdateOrdenVentaCommand() { OrdenMesa = request.OrdenMesa }, tokenCancelacion);
        }

        var orden = await _ordenRepository.Query()
                    .Include(o => o.ListaDetalles.Where(detalle => !detalle.Eliminado))
                    .FirstOrDefaultAsync(ordenActual => !ordenActual.Eliminado && ordenActual.Id == ordenId, tokenCancelacion)
                    ?? throw new InvalidOperationException("Error al crear la orden de mesa.");

        var usoMesa = await _usoMesaRepository.Query().FirstOrDefaultAsync(uso => !uso.Eliminado && uso.IdOrdenVenta == orden.Id && uso.IdMesa == request.OrdenMesa.IdMesa,
                tokenCancelacion) ?? throw new InvalidOperationException("Error al crear el uso de la mesa.");

        var ordenMesaResponse = OrdenMesaUtils.Mapear(orden, usoMesa, orden.ListaDetalles);
        return new Response<OrdenMesaDTO>(ordenMesaResponse);
    }

    #region Validators
    private static void ValidarSolicitud(OrdenMesaDTO ordenMesa)
    {
        if (ordenMesa.IdMesa <= 0) throw new InvalidOperationException("Debe seleccionar una mesa.");

        if (ordenMesa.IdVendedor <= 0) throw new InvalidOperationException("Debe existir un vendedor para guardar la orden.");

        if (ordenMesa.DescuentoGlobal < 0 || ordenMesa.RecargoGlobal < 0) throw new InvalidOperationException("El descuento y el recargo no pueden ser negativos.");

        foreach (var detalle in ordenMesa.Detalles)
        {
            if (detalle.IdProducto <= 0 || detalle.Cantidad <= 0 || detalle.PrecioUnitario < 0)
                throw new InvalidOperationException("Los detalles de la orden contienen valores inválidos.");

            if (detalle.Descuento < 0)
                throw new InvalidOperationException("El descuento de un detalle no puede ser negativo.");
        }
    }

    private async Task ValidarProductos(IEnumerable<OrdenMesaDetalleDTO> detalles, CancellationToken tokenCancelacion)
    {
        var ids = detalles.Select(detalle => detalle.IdProducto).Distinct().ToList();
        if (ids.Count == 0) return;

        var cantidadProductosValidos = await _productoRepository.Query()
            .CountAsync(producto => ids.Contains(producto.Id) && producto.Activo && !producto.Eliminado, tokenCancelacion);

        if (cantidadProductosValidos != ids.Count) throw new InvalidOperationException("La orden contiene productos inexistentes o inactivos.");
    }
    #endregion
}
