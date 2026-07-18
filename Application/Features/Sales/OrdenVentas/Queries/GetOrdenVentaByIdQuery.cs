using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sales.OrdenVentas.Queries;

public class GetOrdenVentaByIdQuery : IQuery<Response<OrdenVentaDTO>>
{
    public required long Id { get; set; }
}

public class GetOrdenVentaByIdQueryHandler : IQueryHandler<GetOrdenVentaByIdQuery, Response<OrdenVentaDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<OrdenVenta> _repository;

    public GetOrdenVentaByIdQueryHandler(IMapper mapper, IRepository<OrdenVenta> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<OrdenVentaDTO>> Handle(GetOrdenVentaByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.Query()
            .Where(p => !p.Eliminado)
            .Where(p => p.Id == request.Id)
            .Include(p => p.Cliente);

        var ordenVenta = await query.FirstOrDefaultAsync(cancellationToken);
        if (ordenVenta == null) throw new Exception("Orden de venta no encontrada.");

        var ordenVentaDto = _mapper.Map<OrdenVentaDTO>(ordenVenta);
        return new Response<OrdenVentaDTO>(ordenVentaDto);
    }
}
