using Application.Interfaces;
using Domain.Common;
using Domain.Entities.Contact;
using Infraestructure.Interfaces;

namespace Application.Features.Contact.Clientes.Commands;

public class DeleteClienteCommand : ICommand<Response<bool>>
{
    public long ClienteId { get; set; }
}

public class DeleteClienteCommandHandler : ICommandHandler<DeleteClienteCommand, Response<bool>>
{
    private readonly IRepository<Cliente> _repository;

    public DeleteClienteCommandHandler(IRepository<Cliente> repository)
    {
        _repository = repository;
    }

    public async Task<Response<bool>> Handle(DeleteClienteCommand request, CancellationToken cancellationToken)
    {
        var cliente = await _repository.GetByIdAsync(request.ClienteId);
        if (cliente == null) throw new ArgumentException("El cliente no existe.");

        _repository.Update(cliente);
        cliente.Eliminado = true;

        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}
