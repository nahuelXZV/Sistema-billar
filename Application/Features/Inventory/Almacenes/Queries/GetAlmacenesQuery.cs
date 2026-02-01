using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;
using Domain.Entities.Inventory;
using Domain.Extensions;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.Almacenes.Queries;

public class GetAlmacenesQuery : ICommand<Response<ResponseFilterDTO<AlmacenDTO>>>
{
    public FilterDTO? Filter { get; set; }
}

public class GetAlmacenesHandler : ICommandHandler<GetAlmacenesQuery, Response<ResponseFilterDTO<AlmacenDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Almacen> _repository;

    public GetAlmacenesHandler(IMapper mapper, IRepository<Almacen> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<ResponseFilterDTO<AlmacenDTO>>> Handle(GetAlmacenesQuery request, CancellationToken cancellationToken)
    {
        var baseQuery = _repository.Query().Where(p => !p.Eliminado);

        var total = await baseQuery.CountAsync(cancellationToken);

        var query = baseQuery.ApplyFilter(
                request.Filter,
                p => string.IsNullOrEmpty(request.Filter.Search)
                     || p.Nombre.ToLower().Contains(request.Filter.Search.ToLower())
                     || p.Descripcion.ToLower().Contains(request.Filter.Search.ToLower())
            );

        var listaAlmacenes = await query.ToListAsync(cancellationToken);
        var listaAlmacenesDTO = _mapper.Map<List<AlmacenDTO>>(listaAlmacenes);

        var response = new ResponseFilterDTO<AlmacenDTO>
        {
            Data = listaAlmacenesDTO,
            Total = total
        };

        return new Response<ResponseFilterDTO<AlmacenDTO>>(response);
    }
}
