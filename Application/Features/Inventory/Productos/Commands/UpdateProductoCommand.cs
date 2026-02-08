using Application.Features.Inventory.Categorias.Commands;
using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.Productos.Commands;

public class UpdateProductoCommand : ICommand<Response<bool>>
{
    public required ProductoDTO ProductoDTO { get; set; }
}

public class UpdateProductoCommandHandler : ICommandHandler<UpdateProductoCommand, Response<bool>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Producto> _repository;
    private readonly IRepository<ProductoCompuesto> _rpProductoCompuesto;

    public UpdateProductoCommandHandler(IMapper mapper, IRepository<Producto> repository, IRepository<ProductoCompuesto> rpProductoCompuesto)
    {
        _mapper = mapper;
        _repository = repository;
        _rpProductoCompuesto = rpProductoCompuesto;
    }

    public async Task<Response<bool>> Handle(UpdateProductoCommand request, CancellationToken cancellationToken)
    {
        var producto = await _repository.GetByIdAsync(request.ProductoDTO.Id);
        if (producto == null) throw new Exception("El producto no existe.");

        _repository.Attach(producto);
        _mapper.Map(request.ProductoDTO, producto);

        if (producto.EsCompuesto)
        {
            var productosCompuestosExistentes = await _rpProductoCompuesto.Query().Where(pc => pc.IdProductoPadre == producto.Id).ToListAsync();
            if (productosCompuestosExistentes.Any())
                _rpProductoCompuesto.DeleteRange(productosCompuestosExistentes);

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
        await _rpProductoCompuesto.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}


