using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using MediatR;

namespace Application.Features.Inventory.Productos.Commands;

public class CreateProductoCommand : ICommand<Response<long>>
{
    public required ProductoDTO ProductoDTO { get; set; }
}

public class CreateProductoCommandHandler : ICommandHandler<CreateProductoCommand, Response<long>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IRepository<Producto> _repository;
    private readonly IRepository<ProductoCompuesto> _rpProductoCompuesto;

    public CreateProductoCommandHandler(IMediator mediator, IMapper mapper, IRepository<Producto> repository, IRepository<ProductoCompuesto> rpProductoCompuesto)
    {
        _mediator = mediator;
        _mapper = mapper;
        _repository = repository;
        _rpProductoCompuesto = rpProductoCompuesto;
    }

    public async Task<Response<long>> Handle(CreateProductoCommand request, CancellationToken cancellationToken)
    {
        Producto producto = _mapper.Map<Producto>(request.ProductoDTO);

        producto.FechaCreacion = DateTime.Now;
        producto = await _repository.AddAsync(producto);

        if (producto.EsCompuesto)
        {
            var productosCompuestos = request.ProductoDTO.ProductosCompuestos?.Select(pc => new ProductoCompuesto
            {
                IdProductoPadre = producto.Id,
                IdProductoComponente = pc.IdProductoComponente,
                Cantidad = pc.Cantidad
            }).ToList();

            if (productosCompuestos != null && productosCompuestos.Any())
                await _rpProductoCompuesto.AddRangeAsync(productosCompuestos);
        }

        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<long>(producto.Id);
    }
}
