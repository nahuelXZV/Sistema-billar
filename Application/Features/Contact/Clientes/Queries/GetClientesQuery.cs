using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Contact;
using Domain.Entities.Contact;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Contact.Clientes.Queries;

public class GetClientesQuery : ICommand<Response<List<ClienteDTO>>>
{
}

public class GetClientesQueryHandler : ICommandHandler<GetClientesQuery, Response<List<ClienteDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Cliente> _repository;

    public GetClientesQueryHandler(IMapper mapper, IRepository<Cliente> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<List<ClienteDTO>>> Handle(GetClientesQuery request, CancellationToken cancellationToken)
    {
        var listaClientes = await _repository.Query().Where(p => !p.Eliminado).ToListAsync(cancellationToken);
        var listaClientesDTO = _mapper.Map<List<ClienteDTO>>(listaClientes);
        return new Response<List<ClienteDTO>>(listaClientesDTO);
    }
}
