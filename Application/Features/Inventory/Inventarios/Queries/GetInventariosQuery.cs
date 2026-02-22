using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace Application.Features.Inventory.Inventarios.Queries;

public class GetInventariosQuery : ICommand<Response<List<InventarioDTO>>>
{
    public bool ConStock { get; set; } = true;
}

public class GetInventariosHandler : ICommandHandler<GetInventariosQuery, Response<List<InventarioDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Inventario> _repository;

    public GetInventariosHandler(IMapper mapper, IRepository<Inventario> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<List<InventarioDTO>>> Handle(GetInventariosQuery request, CancellationToken cancellationToken)
    {
        var inventario = await _repository.Query()
            .Where(p => !p.Eliminado)
            .Where(p => request.ConStock ? p.Cantidad > 0 : true).ToListAsync();

        var inventarioDto = _mapper.Map<List<InventarioDTO>>(inventario);
        return new Response<List<InventarioDTO>>(inventarioDto);
    }
}

