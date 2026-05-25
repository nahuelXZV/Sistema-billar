using Application.Interfaces;
using Domain.Common;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sales.Vendedores.Commands;

public class DeleteVendedorCommand : ICommand<Response<bool>>
{
    public long VendedorId { get; set; }
}

public class DeleteVendedorCommandHandler : ICommandHandler<DeleteVendedorCommand, Response<bool>>
{
    private readonly IRepository<Vendedor> _repository;
    private readonly IRepository<VendedorAlmacenes> _vendedorAlmacenRepository;

    public DeleteVendedorCommandHandler(
        IRepository<Vendedor> repository,
        IRepository<VendedorAlmacenes> vendedorAlmacenRepository)
    {
        _repository = repository;
        _vendedorAlmacenRepository = vendedorAlmacenRepository;
    }

    public async Task<Response<bool>> Handle(DeleteVendedorCommand request, CancellationToken cancellationToken)
    {
        var vendedor = await _repository.GetByIdAsync(request.VendedorId);
        if (vendedor == null) throw new ArgumentException("El vendedor no existe.");

        _repository.Delete(vendedor);

        var relaciones = await _vendedorAlmacenRepository.Query()
            .Where(p => p.IdVendedor == request.VendedorId && !p.Eliminado)
            .ToListAsync(cancellationToken);

        if (relaciones.Count > 0)
        {
            _vendedorAlmacenRepository.DeleteRange(relaciones);
        }

        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}
