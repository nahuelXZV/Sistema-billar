using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Purchases;
using Domain.DTOs.Shared;
using Domain.Entities.Purchases;
using Domain.Extensions;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Purchases.Compras.Queries;

public class GetComprasFilterQuery : IQuery<Response<ResponseFilterDTO<CompraDTO>>>
{
    public FilterDTO? Filter { get; set; }
}

public class GetComprasFilterQueryHandler : IQueryHandler<GetComprasFilterQuery, Response<ResponseFilterDTO<CompraDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Compra> _repository;

    public GetComprasFilterQueryHandler(IMapper mapper, IRepository<Compra> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<ResponseFilterDTO<CompraDTO>>> Handle(
        GetComprasFilterQuery request,
        CancellationToken cancellationToken)
    {
        var baseQuery = _repository.Query().Where(compra => !compra.Eliminado);
        var total = await baseQuery.CountAsync(cancellationToken);
        var search = request.Filter?.Search?.Trim().ToLower();

        var compras = await GetComprasQueryHandler.IncluirRelaciones(baseQuery)
            .ApplyFilter(
                request.Filter,
                compra => string.IsNullOrEmpty(search)
                    || compra.Numero.ToLower().Contains(search)
                    || (compra.Proveedor != null &&
                        compra.Proveedor.NombreComercial != null &&
                        compra.Proveedor.NombreComercial.ToLower().Contains(search)))
            .OrderByDescending(o => o.Fecha)
            .ToListAsync(cancellationToken);

        var response = new ResponseFilterDTO<CompraDTO>
        {
            Data = _mapper.Map<List<CompraDTO>>(compras),
            Total = total
        };

        return new Response<ResponseFilterDTO<CompraDTO>>(response);
    }
}
