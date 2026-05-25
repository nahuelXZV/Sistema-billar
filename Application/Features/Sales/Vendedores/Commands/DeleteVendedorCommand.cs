using Application.Interfaces;
using Domain.Common;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;

namespace Application.Features.Sales.Vendedores.Commands;

public class DeleteVendedorCommand : ICommand<Response<bool>>
{
    public long VendedorId { get; set; }
}

public class DeleteVendedorCommandHandler : ICommandHandler<DeleteVendedorCommand, Response<bool>>
{
    private readonly IRepository<Vendedor> _repository;

    public DeleteVendedorCommandHandler(IRepository<Vendedor> repository)
    {
        _repository = repository;
    }

    public async Task<Response<bool>> Handle(DeleteVendedorCommand request, CancellationToken cancellationToken)
    {
        var vendedor = await _repository.GetByIdAsync(request.VendedorId);
        if (vendedor == null) throw new ArgumentException("El vendedor no existe.");

        _repository.Update(vendedor);
        vendedor.Eliminado = true;

        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}
