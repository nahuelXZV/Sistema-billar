using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.ListaPrecio.Commands;

public class UpdateListaPreciosCommand : ICommand<Response<bool>>
{
    public required ListaPrecioDTO ListaPrecioDTO { get; set; }
}

public class UpdateListaPreciosHandler : ICommandHandler<UpdateListaPreciosCommand, Response<bool>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<ListaPrecios> _repository;
    private readonly IRepository<ListaPreciosDetalle> _rpDetalles;
    private readonly IRepository<ProductoConversion> _rpProductoConversion;

    public UpdateListaPreciosHandler(
        IMapper mapper,
        IRepository<ListaPrecios> repository,
        IRepository<ListaPreciosDetalle> rpDetalles,
        IRepository<ProductoConversion> rpProductoConversion)
    {
        _mapper = mapper;
        _repository = repository;
        _rpDetalles = rpDetalles;
        _rpProductoConversion = rpProductoConversion;
    }

    public async Task<Response<bool>> Handle(UpdateListaPreciosCommand request, CancellationToken cancellationToken)
    {
        var detalles = request.ListaPrecioDTO.ListaDetalles?.ToList() ?? [];
        await ValidarDetallesAsync(detalles, cancellationToken);

        var listaPrecio = await _repository.GetByIdAsync(request.ListaPrecioDTO.Id);
        if (listaPrecio == null) throw new ArgumentException("La lista de precios no existe.");

        var detallesGuardados = await _rpDetalles.Query()
            .Where(detalle =>
                detalle.IdListaPrecio == request.ListaPrecioDTO.Id &&
                !detalle.Eliminado)
            .ToListAsync(cancellationToken);

        if (detallesGuardados.Count > 0)
        {
            _rpDetalles.DeleteRange(detallesGuardados);
            await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        }

        _repository.Attach(listaPrecio);
        _mapper.Map(request.ListaPrecioDTO, listaPrecio);

        if (detalles.Count > 0)
        {
            await _rpDetalles.AddRangeAsync(detalles.Select(detalle => new ListaPreciosDetalle
            {
                IdListaPrecio = listaPrecio.Id,
                IdProductoConversion = detalle.IdProductoConversion,
                Precio = detalle.Precio
            }));
        }

        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }

    private async Task ValidarDetallesAsync(IReadOnlyCollection<ListaPrecioDetalleDTO> detalles, CancellationToken cancellationToken)
    {
        if (detalles.Any(detalle => detalle.IdProductoConversion <= 0))
            throw new InvalidOperationException("Todos los precios deben tener una unidad de medida seleccionada.");

        if (detalles.Any(detalle => detalle.Precio <= 0))
            throw new InvalidOperationException("Todos los precios deben ser mayores a cero.");

        var idsConversiones = detalles
            .Select(detalle => detalle.IdProductoConversion)
            .ToList();

        if (idsConversiones.Distinct().Count() != idsConversiones.Count)
            throw new InvalidOperationException("No se puede repetir la misma unidad de un producto en la lista de precios.");

        var totalConversionesValidas = await _rpProductoConversion.Query()
            .CountAsync(conversion => !conversion.Eliminado && idsConversiones.Contains(conversion.Id), cancellationToken);

        if (totalConversionesValidas != idsConversiones.Count)
            throw new InvalidOperationException("Una o más unidades configuradas ya no están disponibles.");
    }
}
