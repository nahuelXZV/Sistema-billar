using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sales.OrdenVentas.Queries;

public class GetOrdenVentasQuery : IQuery<Response<List<OrdenVentaDTO>>>
{
}

public class GetOrdenVentasQueryHandler : IQueryHandler<GetOrdenVentasQuery, Response<List<OrdenVentaDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<OrdenVenta> _repository;

    public GetOrdenVentasQueryHandler(IMapper mapper, IRepository<OrdenVenta> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<List<OrdenVentaDTO>>> Handle(GetOrdenVentasQuery request, CancellationToken cancellationToken)
    {
        var listaOrdenes = await _repository.Query()
            .Include(p => p.Cliente)
            .Where(p => !p.Eliminado)
            .ToListAsync(cancellationToken);

        var listaOrdenesDto = _mapper.Map<List<OrdenVentaDTO>>(listaOrdenes);
        return new Response<List<OrdenVentaDTO>>(listaOrdenesDto);
    }
}
