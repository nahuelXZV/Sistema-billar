using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Configuration;
using Domain.DTOs.Shared;
using Domain.Entities.Configuration;
using Domain.Extensions;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Configuration.Mesas.Queries;

public class GetMesasFilterQuery : IQuery<Response<ResponseFilterDTO<MesaDTO>>>
{
    public FilterDTO? Filter { get; set; }
}

public class GetMesasFilterQueryHandler : IQueryHandler<GetMesasFilterQuery, Response<ResponseFilterDTO<MesaDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Mesa> _repository;

    public GetMesasFilterQueryHandler(IMapper mapper, IRepository<Mesa> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<ResponseFilterDTO<MesaDTO>>> Handle(GetMesasFilterQuery request, CancellationToken cancellationToken)
    {
        var baseQuery = _repository.Query()
            .Include(p => p.TipoMesa)
            .Where(p => !p.Eliminado);
        var search = request.Filter?.Search;

        var total = await baseQuery.CountAsync(cancellationToken);

        var query = baseQuery.ApplyFilter(
                request.Filter,
                p => string.IsNullOrEmpty(search)
                     || p.Nombre.ToLower().Contains(search.ToLower())
            );

        var listaMesas = await query.ToListAsync(cancellationToken);
        var listaMesasDtos = _mapper.Map<List<MesaDTO>>(listaMesas);

        var response = new ResponseFilterDTO<MesaDTO>
        {
            Data = listaMesasDtos,
            Total = total
        };

        return new Response<ResponseFilterDTO<MesaDTO>>(response);
    }
}
