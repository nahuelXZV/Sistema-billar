using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;
using Domain.Entities.Inventory;
using Domain.Extensions;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.TraspasoInventarios.Queries;

public class GetTraspasosInventarioFilterQuery : IQuery<Response<ResponseFilterDTO<TraspasoInventarioDTO>>>
{
    public FilterDTO? Filter { get; set; }
}

public class GetTraspasosInventarioFilterHandler : IQueryHandler<GetTraspasosInventarioFilterQuery, Response<ResponseFilterDTO<TraspasoInventarioDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<TraspasoInventario> _repository;

    public GetTraspasosInventarioFilterHandler(IMapper mapper, IRepository<TraspasoInventario> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<ResponseFilterDTO<TraspasoInventarioDTO>>> Handle(GetTraspasosInventarioFilterQuery request, CancellationToken cancellationToken)
    {
        var baseQuery = _repository.Query()
            .Where(traspaso => !traspaso.Eliminado)
            .Include(traspaso => traspaso.AlmacenOrigen)
            .Include(traspaso => traspaso.AlmacenDestino);

        var total = await baseQuery.CountAsync(cancellationToken);
        var query = baseQuery.ApplyFilter(
            request.Filter,
            traspaso => string.IsNullOrEmpty(request.Filter!.Search)
                || traspaso.Id.ToString().Contains(request.Filter.Search)
                || traspaso.Glosa.ToLower().Contains(request.Filter.Search.ToLower())
                || traspaso.AlmacenOrigen!.Nombre.ToLower().Contains(request.Filter.Search.ToLower())
                || traspaso.AlmacenDestino!.Nombre.ToLower().Contains(request.Filter.Search.ToLower()));

        var traspasos = await query
            .OrderByDescending(traspaso => traspaso.Id)
            .ToListAsync(cancellationToken);

        return new Response<ResponseFilterDTO<TraspasoInventarioDTO>>(new ResponseFilterDTO<TraspasoInventarioDTO>
        {
            Data = _mapper.Map<List<TraspasoInventarioDTO>>(traspasos),
            Total = total
        });
    }
}
