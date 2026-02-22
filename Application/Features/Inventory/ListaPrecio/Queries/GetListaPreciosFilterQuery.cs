using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;
using Domain.Entities.Inventory;
using Domain.Extensions;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace Application.Features.Inventory.ListaPrecio.Queries;

public class GetListaPreciosFilterQuery : ICommand<Response<ResponseFilterDTO<ListaPrecioDTO>>>
{
    public FilterDTO? Filter { get; set; }
}

public class GetListaPreciosFilterHandler : ICommandHandler<GetListaPreciosFilterQuery, Response<ResponseFilterDTO<ListaPrecioDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<ListaPrecios> _repository;

    public GetListaPreciosFilterHandler(IMapper mapper, IRepository<ListaPrecios> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<ResponseFilterDTO<ListaPrecioDTO>>> Handle(GetListaPreciosFilterQuery request, CancellationToken cancellationToken)
    {
        var baseQuery = _repository.Query().Where(p => !p.Eliminado)
            .Include(p => p.ListaDetalles);

        var total = await baseQuery.CountAsync(cancellationToken);

        var query = baseQuery.ApplyFilter(
                request.Filter,
                p => string.IsNullOrEmpty(request.Filter.Search)
                     || p.Nombre.ToLower().Contains(request.Filter.Search.ToLower())
                     || p.Descripcion.ToLower().Contains(request.Filter.Search.ToLower())
            );

        var listaPrecios = await query.ToListAsync(cancellationToken);
        var listaPreciosDtos = _mapper.Map<List<ListaPrecioDTO>>(listaPrecios);

        var response = new ResponseFilterDTO<ListaPrecioDTO>
        {
            Data = listaPreciosDtos,
            Total = total
        };
        return new Response<ResponseFilterDTO<ListaPrecioDTO>>(response);
    }
}

