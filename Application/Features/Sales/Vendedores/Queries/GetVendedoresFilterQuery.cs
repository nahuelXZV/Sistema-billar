using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.DTOs.Shared;
using Domain.Entities.Sales;
using Domain.Extensions;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sales.Vendedores.Queries;

public class GetVendedoresFilterQuery : ICommand<Response<ResponseFilterDTO<VendedorDTO>>>
{
    public FilterDTO? Filter { get; set; }
}

public class GetVendedoresFilterQueryHandler : ICommandHandler<GetVendedoresFilterQuery, Response<ResponseFilterDTO<VendedorDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Vendedor> _repository;

    public GetVendedoresFilterQueryHandler(IMapper mapper, IRepository<Vendedor> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<ResponseFilterDTO<VendedorDTO>>> Handle(GetVendedoresFilterQuery request, CancellationToken cancellationToken)
    {
        var baseQuery = _repository.Query()
            .Include(p => p.Usuario)
            .Include(p => p.ListaPrecio)
            .Include(p => p.ListaAlmacenes.Where(a => !a.Eliminado))
            .ThenInclude(a => a.Almacen)
            .Where(p => !p.Eliminado);

        var search = request.Filter?.Search;

        var total = await baseQuery.CountAsync(cancellationToken);

        var query = baseQuery.ApplyFilter(
                request.Filter,
                p => string.IsNullOrEmpty(search)
                     || p.Nombre.ToLower().Contains(search.ToLower())
                     || p.Documento.ToLower().Contains(search.ToLower())
            );

        var listaVendedores = await query.ToListAsync(cancellationToken);
        var listaVendedoresDTO = _mapper.Map<List<VendedorDTO>>(listaVendedores);

        var response = new ResponseFilterDTO<VendedorDTO>
        {
            Data = listaVendedoresDTO,
            Total = total
        };

        return new Response<ResponseFilterDTO<VendedorDTO>>(response);
    }
}
