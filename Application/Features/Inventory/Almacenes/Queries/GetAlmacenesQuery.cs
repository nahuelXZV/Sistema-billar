using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.Almacenes.Queries;

public class GetAlmacenesQuery : ICommand<Response<List<AlmacenDTO>>>
{
}

public class GetAlmacenesHandler : ICommandHandler<GetAlmacenesQuery, Response<List<AlmacenDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Almacen> _repository;

    public GetAlmacenesHandler(IMapper mapper, IRepository<Almacen> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<List<AlmacenDTO>>> Handle(GetAlmacenesQuery request, CancellationToken cancellationToken)
    {
        var listaAlmacenes = await _repository.Query().Where(p => !p.Eliminado).ToListAsync(cancellationToken);
        var listaAlmacenesDTO = _mapper.Map<List<AlmacenDTO>>(listaAlmacenes);
        return new Response<List<AlmacenDTO>>(listaAlmacenesDTO);
    }
}
