using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Contact;
using Domain.Entities.Contact;
using Infraestructure.Interfaces;
using MediatR;

namespace Application.Features.Contact.Clientes.Commands;

public class CreateClienteCommand : ICommand<Response<long>>
{
    public required ClienteDTO ClienteDTO { get; set; }
}

public class CreateClienteCommandHandler : ICommandHandler<CreateClienteCommand, Response<long>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IRepository<Cliente> _repository;

    public CreateClienteCommandHandler(IMediator mediator, IMapper mapper, IRepository<Cliente> repository)
    {
        _mediator = mediator;
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<long>> Handle(CreateClienteCommand request, CancellationToken cancellationToken)
    {
        Cliente cliente = _mapper.Map<Cliente>(request.ClienteDTO);
        cliente = await _repository.AddAsync(cliente);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<long>(cliente.Id);
    }
}
