using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Contact;
using Domain.DTOs.Shared;
using Domain.Entities.Contact;
using Domain.Extensions;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Contact.Clientes.Queries;

public class GetClientesFilterQuery : ICommand<Response<ResponseFilterDTO<ClienteDTO>>>
{
    public FilterDTO? Filter { get; set; }
}

public class GetClientesFilterQueryHandler : ICommandHandler<GetClientesFilterQuery, Response<ResponseFilterDTO<ClienteDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Cliente> _repository;

    public GetClientesFilterQueryHandler(IMapper mapper, IRepository<Cliente> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<ResponseFilterDTO<ClienteDTO>>> Handle(GetClientesFilterQuery request, CancellationToken cancellationToken)
    {
        var baseQuery = _repository.Query().Where(p => !p.Eliminado);
        var search = request.Filter?.Search;

        var total = await baseQuery.CountAsync(cancellationToken);

        var query = baseQuery.ApplyFilter(
                request.Filter,
                p => string.IsNullOrEmpty(search)
                     || p.Nombre.ToLower().Contains(search.ToLower())
                     || p.Documento.ToLower().Contains(search.ToLower())
                     || p.Telefono.ToLower().Contains(search.ToLower())
            );

        var listaClientes = await query.ToListAsync(cancellationToken);
        var listaClientesDTO = _mapper.Map<List<ClienteDTO>>(listaClientes);

        var response = new ResponseFilterDTO<ClienteDTO>
        {
            Data = listaClientesDTO,
            Total = total
        };

        return new Response<ResponseFilterDTO<ClienteDTO>>(response);
    }
}
