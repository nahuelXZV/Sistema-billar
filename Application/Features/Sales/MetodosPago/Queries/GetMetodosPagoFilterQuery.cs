using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.DTOs.Shared;
using Domain.Entities.Sales;
using Domain.Extensions;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sales.MetodosPago.Queries;

public class GetMetodosPagoFilterQuery : IQuery<Response<ResponseFilterDTO<MetodoPagoDTO>>>
{
    public FilterDTO? Filter { get; set; }
}

public class GetMetodosPagoFilterQueryHandler : IQueryHandler<GetMetodosPagoFilterQuery, Response<ResponseFilterDTO<MetodoPagoDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<MetodoPago> _repository;

    public GetMetodosPagoFilterQueryHandler(IMapper mapper, IRepository<MetodoPago> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<ResponseFilterDTO<MetodoPagoDTO>>> Handle(GetMetodosPagoFilterQuery request, CancellationToken cancellationToken)
    {
        var baseQuery = _repository.Query().Where(p => !p.Eliminado);
        var search = request.Filter?.Search;

        var total = await baseQuery.CountAsync(cancellationToken);

        var query = baseQuery.ApplyFilter(
            request.Filter,
            p => string.IsNullOrEmpty(search)
                 || p.Nombre.ToLower().Contains(search.ToLower())
                 || p.Abreviatura.ToLower().Contains(search.ToLower())
                 || p.ClaveMoneda.ToLower().Contains(search.ToLower())
                 || p.Icono.ToLower().Contains(search.ToLower())
        );

        var metodosPago = await query.ToListAsync(cancellationToken);
        var metodosPagoDto = _mapper.Map<List<MetodoPagoDTO>>(metodosPago);

        var response = new ResponseFilterDTO<MetodoPagoDTO>
        {
            Data = metodosPagoDto,
            Total = total
        };

        return new Response<ResponseFilterDTO<MetodoPagoDTO>>(response);
    }
}
