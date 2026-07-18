using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.DTOs.Shared;
using Domain.Entities.Sales;
using Domain.Extensions;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sales.Ventas.Queries;

public class GetVentasFilterQuery : IQuery<Response<ResponseFilterDTO<VentaDTO>>>
{
    public FilterDTO? Filter { get; set; }
}

public class GetVentasFilterQueryHandler : IQueryHandler<GetVentasFilterQuery, Response<ResponseFilterDTO<VentaDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Venta> _repository;

    public GetVentasFilterQueryHandler(IMapper mapper, IRepository<Venta> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<ResponseFilterDTO<VentaDTO>>> Handle(GetVentasFilterQuery request, CancellationToken cancellationToken)
    {
        var baseQuery = _repository.Query()
            .Include(p => p.OrdenVenta)
            .Include(p => p.Cliente)
            .Include(p => p.Vendedor)
            .Where(p => !p.Eliminado);

        var search = request.Filter?.Search;

        var query = baseQuery;

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p =>
                p.Numero.ToLower().Contains(search.ToLower())
                || p.Observacion.ToLower().Contains(search.ToLower())
                || (p.Cliente != null && p.Cliente.Nombre.ToLower().Contains(search.ToLower()))
                || (p.Vendedor != null && p.Vendedor.Nombre.ToLower().Contains(search.ToLower())));
        }

        var total = await query.CountAsync(cancellationToken);

        query = query
            .OrderByDescending(q => q.Id)
            .ApplyFilter(request.Filter);

        var listaVentas = await query.ToListAsync(cancellationToken);
        var listaVentasDto = _mapper.Map<List<VentaDTO>>(listaVentas);

        var response = new ResponseFilterDTO<VentaDTO>
        {
            Data = listaVentasDto,
            Total = total
        };

        return new Response<ResponseFilterDTO<VentaDTO>>(response);
    }
}
