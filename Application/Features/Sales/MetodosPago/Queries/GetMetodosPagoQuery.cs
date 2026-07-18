using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sales.MetodosPago.Queries;

public class GetMetodosPagoQuery : IQuery<Response<List<MetodoPagoDTO>>>
{
}

public class GetMetodosPagoQueryHandler : IQueryHandler<GetMetodosPagoQuery, Response<List<MetodoPagoDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<MetodoPago> _repository;

    public GetMetodosPagoQueryHandler(IMapper mapper, IRepository<MetodoPago> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<List<MetodoPagoDTO>>> Handle(GetMetodosPagoQuery request, CancellationToken cancellationToken)
    {
        var metodosPago = await _repository.Query()
            .Where(p => !p.Eliminado)
            .Where(p => p.Activo)
            .ToListAsync(cancellationToken);

        var metodosPagoDto = _mapper.Map<List<MetodoPagoDTO>>(metodosPago);
        return new Response<List<MetodoPagoDTO>>(metodosPagoDto);
    }
}
