using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sales.Ventas.Queries;

public class GetVentaByIdQuery : ICommand<Response<VentaDTO>>
{
    public required long Id { get; set; }
}

public class GetVentaByIdQueryHandler : ICommandHandler<GetVentaByIdQuery, Response<VentaDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Venta> _repository;

    public GetVentaByIdQueryHandler(IMapper mapper, IRepository<Venta> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<VentaDTO>> Handle(GetVentaByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.Query()
            .Where(p => !p.Eliminado)
            .Where(p => p.Id == request.Id)
            .Include(p => p.OrdenVenta)
            .Include(p => p.Cliente)
            .Include(p => p.Vendedor)
            .Include(p => p.ListaDetalles)
                .ThenInclude(d => d.Producto)
            .Include(p => p.ListaPagos)
                .ThenInclude(p => p.MetodoPago);

        var venta = await query.FirstOrDefaultAsync(cancellationToken);
        if (venta == null) throw new Exception("Venta no encontrada.");

        var ventaDto = _mapper.Map<VentaDTO>(venta);
        return new Response<VentaDTO>(ventaDto);
    }
}
