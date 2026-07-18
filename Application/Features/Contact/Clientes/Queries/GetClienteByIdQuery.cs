using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Contact;
using Domain.Entities.Contact;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Contact.Clientes.Queries;

public class GetClienteByIdQuery : IQuery<Response<ClienteDTO>>
{
    public required long Id { get; set; }
}

public class GetClienteByIdQueryHandler : IQueryHandler<GetClienteByIdQuery, Response<ClienteDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Cliente> _repository;

    public GetClienteByIdQueryHandler(IMapper mapper, IRepository<Cliente> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<ClienteDTO>> Handle(GetClienteByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.Query()
            .Where(p => !p.Eliminado)
            .Where(p => p.Id == request.Id);

        var cliente = await query.FirstOrDefaultAsync(cancellationToken);
        if (cliente == null) throw new Exception("Cliente no encontrado.");

        var clienteDTO = _mapper.Map<ClienteDTO>(cliente);
        return new Response<ClienteDTO>(clienteDTO);
    }
}
