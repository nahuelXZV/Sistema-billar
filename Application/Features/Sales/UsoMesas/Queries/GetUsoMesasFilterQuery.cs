using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.DTOs.Shared;
using Domain.Entities.Sales;
using Domain.Extensions;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sales.UsoMesas.Queries;

public class GetUsoMesasFilterQuery : IQuery<Response<ResponseFilterDTO<UsoMesaDTO>>>
{
    public FilterDTO? Filter { get; set; }
}

public class GetUsoMesasFilterQueryHandler : IQueryHandler<GetUsoMesasFilterQuery, Response<ResponseFilterDTO<UsoMesaDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<UsoMesa> _repository;

    public GetUsoMesasFilterQueryHandler(IMapper mapper, IRepository<UsoMesa> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<ResponseFilterDTO<UsoMesaDTO>>> Handle(GetUsoMesasFilterQuery request, CancellationToken cancellationToken)
    {
        var baseQuery = _repository.Query()
            .Include(p => p.Mesa)
            .Where(p => !p.Eliminado);

        var search = request.Filter?.Search;

        var total = await baseQuery.CountAsync(cancellationToken);

        var query = baseQuery.ApplyFilter(
                request.Filter,
                p => string.IsNullOrEmpty(search)
                     || p.Observacion.ToLower().Contains(search.ToLower())
                     || p.Mesa!.Nombre.ToLower().Contains(search.ToLower())
            );

        var listaUsoMesas = await query.ToListAsync(cancellationToken);
        var listaUsoMesasDto = _mapper.Map<List<UsoMesaDTO>>(listaUsoMesas);

        var response = new ResponseFilterDTO<UsoMesaDTO>
        {
            Data = listaUsoMesasDto,
            Total = total
        };

        return new Response<ResponseFilterDTO<UsoMesaDTO>>(response);
    }
}
