using Application.Interfaces;
using Domain.Common;
using Domain.Entities.Contact;
using Domain.Entities.Purchases;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Contact.Proveedores.Commands;

public class DeleteProveedorCommand : ICommand<Response<bool>>
{
    public long ProveedorId { get; set; }
}

public class DeleteProveedorCommandHandler : ICommandHandler<DeleteProveedorCommand, Response<bool>>
{
    private readonly IRepository<Proveedor> _proveedorRepository;
    private readonly IRepository<ProveedorProducto> _proveedorProductoRepository;

    public DeleteProveedorCommandHandler(IRepository<Proveedor> proveedorRepository, IRepository<ProveedorProducto> proveedorProductoRepository)
    {
        _proveedorRepository = proveedorRepository;
        _proveedorProductoRepository = proveedorProductoRepository;
    }

    public async Task<Response<bool>> Handle(DeleteProveedorCommand request, CancellationToken cancellationToken)
    {
        var proveedor = await _proveedorRepository.Query()
            .FirstOrDefaultAsync(item => item.Id == request.ProveedorId && !item.Eliminado, cancellationToken)
            ?? throw new ArgumentException("El proveedor no existe.");

        _proveedorRepository.Delete(proveedor);

        var costos = await _proveedorProductoRepository.Query()
            .Where(costo => costo.IdProveedor == proveedor.Id && !costo.Eliminado)
            .ToListAsync(cancellationToken);

        if (costos.Count > 0)
        {
            _proveedorProductoRepository.DeleteRange(costos);
        }

        await _proveedorRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}
