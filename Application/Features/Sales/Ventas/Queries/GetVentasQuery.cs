using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sales.Ventas.Queries;

public class GetVentasQuery : ICommand<Response<List<VentaDTO>>>
{
}

public class GetVentasQueryHandler : ICommandHandler<GetVentasQuery, Response<List<VentaDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Venta> _repository;

    public GetVentasQueryHandler(IMapper mapper, IRepository<Venta> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<List<VentaDTO>>> Handle(GetVentasQuery request, CancellationToken cancellationToken)
    {
        var listaVentas = await _repository.Query()
            .Include(p => p.OrdenVenta)
            .Include(p => p.Cliente)
            .Include(p => p.Vendedor)
            .Include(p => p.ListaDetalles)
            .Include(p => p.ListaPagos)
            .Where(p => !p.Eliminado)
            .ToListAsync(cancellationToken);

        var listaVentasDto = _mapper.Map<List<VentaDTO>>(listaVentas);
        return new Response<List<VentaDTO>>(listaVentasDto);
    }
}
