using Application.Interfaces;
using Domain.Common;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;

namespace Application.Features.Inventory.Categorias.Commands;

public class DeleteCategoriaCommand : ICommand<Response<bool>>
{
    public long CategoriaId { get; set; }
}

public class DeleteCategoriaHandler : ICommandHandler<DeleteCategoriaCommand, Response<bool>>
{
    private readonly IRepository<Categoria> _repository;

    public DeleteCategoriaHandler(IRepository<Categoria> repository)
    {
        _repository = repository;
    }

    public async Task<Response<bool>> Handle(DeleteCategoriaCommand request, CancellationToken cancellationToken)
    {
        var categoria = await _repository.GetByIdAsync(request.CategoriaId);
        if (categoria == null) throw new ArgumentException("La categoria no existe.");

        _repository.Attach(categoria);
        categoria.Eliminado = true;

        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}
