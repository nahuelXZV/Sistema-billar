using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Contact;
using Domain.Entities.Contact;
using Infraestructure.Interfaces;
using MediatR;

namespace Application.Features.Contact.Clientes.Commands;

public class UpdateClienteCommand : ICommand<Response<bool>>
{
    public required ClienteDTO ClienteDTO { get; set; }
}

public class UpdateClienteCommandHandler : ICommandHandler<UpdateClienteCommand, Response<bool>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IRepository<Cliente> _repository;

    public UpdateClienteCommandHandler(IMediator mediator, IMapper mapper, IRepository<Cliente> repository)
    {
        _mediator = mediator;
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<bool>> Handle(UpdateClienteCommand request, CancellationToken cancellationToken)
    {
        var cliente = await _repository.GetByIdAsync(request.ClienteDTO.Id);
        if (cliente == null) throw new ArgumentException("El cliente no existe.");

        _repository.Update(cliente);
        _mapper.Map(request.ClienteDTO, cliente);

        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}
