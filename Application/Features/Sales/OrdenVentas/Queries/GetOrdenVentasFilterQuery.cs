using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.DTOs.Shared;
using Domain.Entities.Sales;
using Domain.Extensions;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sales.OrdenVentas.Queries;

public class GetOrdenVentasFilterQuery : IQuery<Response<ResponseFilterDTO<OrdenVentaDTO>>>
{
    public FilterDTO? Filter { get; set; }
}

public class GetOrdenVentasFilterQueryHandler : IQueryHandler<GetOrdenVentasFilterQuery, Response<ResponseFilterDTO<OrdenVentaDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<OrdenVenta> _repository;

    public GetOrdenVentasFilterQueryHandler(IMapper mapper, IRepository<OrdenVenta> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<ResponseFilterDTO<OrdenVentaDTO>>> Handle(GetOrdenVentasFilterQuery request, CancellationToken cancellationToken)
    {
        var baseQuery = _repository.Query()
            .Include(p => p.Cliente)
            .Where(p => !p.Eliminado);

        var search = request.Filter?.Search;

        var total = await baseQuery.CountAsync(cancellationToken);

        var query = baseQuery.ApplyFilter(
                request.Filter,
                p => string.IsNullOrEmpty(search)
                     || p.Numero.ToLower().Contains(search.ToLower())
                     || (p.Observacion != null && p.Observacion.ToLower().Contains(search.ToLower()))
                     || (p.Cliente != null && p.Cliente.Nombre.ToLower().Contains(search.ToLower()))
            );

        var listaOrdenes = await query.ToListAsync(cancellationToken);
        var listaOrdenesDto = _mapper.Map<List<OrdenVentaDTO>>(listaOrdenes);

        var response = new ResponseFilterDTO<OrdenVentaDTO>
        {
            Data = listaOrdenesDto,
            Total = total
        };

        return new Response<ResponseFilterDTO<OrdenVentaDTO>>(response);
    }
}
