using Application.Interfaces;
using Domain.Common;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;

namespace Application.Features.Inventory.Productos.Commands;

public class DeleteProductoCommand : ICommand<Response<bool>>
{
    public long ProductoId { get; set; }
}

public class DeleteProductoCommandHandler : ICommandHandler<DeleteProductoCommand, Response<bool>>
{
    private readonly IRepository<Producto> _rp;

    public DeleteProductoCommandHandler(IRepository<Producto> rp)
    {
        _rp = rp;
    }

    public async Task<Response<bool>> Handle(DeleteProductoCommand request, CancellationToken cancellationToken)
    {
        var producto = await _rp.GetByIdAsync(request.ProductoId);
        if (producto == null) throw new ArgumentException("El producto no existe.");

        _rp.Attach(producto);
        producto.Eliminado = true;

        await _rp.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}
