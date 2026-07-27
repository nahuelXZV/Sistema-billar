using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.ListaPrecio.Commands;

public class CreateListaPreciosCommand : ICommand<Response<long>>
{
    public required ListaPrecioDTO ListaPrecioDTO { get; set; }
}

public class CreateListaPreciosHandler : ICommandHandler<CreateListaPreciosCommand, Response<long>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<ListaPrecios> _repository;
    private readonly IRepository<ListaPreciosDetalle> _rpDetalles;
    private readonly IRepository<ProductoConversion> _rpProductoConversion;

    public CreateListaPreciosHandler(
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

    public async Task<Response<long>> Handle(CreateListaPreciosCommand request, CancellationToken cancellationToken)
    {
        var detalles = request.ListaPrecioDTO.ListaDetalles?.ToList() ?? [];
        await ValidarDetallesAsync(detalles, cancellationToken);

        var listaPrecio = _mapper.Map<ListaPrecios>(request.ListaPrecioDTO);
        listaPrecio = await _repository.AddAsync(listaPrecio);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        if (detalles.Count > 0)
        {
            await _rpDetalles.AddRangeAsync(detalles.Select(detalle => new ListaPreciosDetalle
            {
                IdListaPrecio = listaPrecio.Id,
                IdProductoConversion = detalle.IdProductoConversion,
                Precio = detalle.Precio
            }));

            await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        }

        return new Response<long>(listaPrecio.Id);
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
            .CountAsync(
                conversion => !conversion.Eliminado && idsConversiones.Contains(conversion.Id),
                cancellationToken);

        if (totalConversionesValidas != idsConversiones.Count)
            throw new InvalidOperationException("Una o más unidades configuradas ya no están disponibles.");
    }
}
