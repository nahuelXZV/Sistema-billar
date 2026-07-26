using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.TraspasoInventarios.Queries;

public class GetTraspasoInventarioByIdQuery : IQuery<Response<TraspasoInventarioDTO>>
{
    public required long Id { get; set; }
}

public class GetTraspasoInventarioByIdHandler : IQueryHandler<GetTraspasoInventarioByIdQuery, Response<TraspasoInventarioDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<TraspasoInventario> _repository;

    public GetTraspasoInventarioByIdHandler(IMapper mapper, IRepository<TraspasoInventario> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<TraspasoInventarioDTO>> Handle(GetTraspasoInventarioByIdQuery request, CancellationToken cancellationToken)
    {
        var traspaso = await _repository.Query()
            .Include(item => item.AlmacenOrigen)
            .Include(item => item.AlmacenDestino)
            .Include(item => item.Detalles)
                .ThenInclude(detalle => detalle.Producto)
            .Include(item => item.Detalles)
                .ThenInclude(detalle => detalle.Lote)
            .FirstOrDefaultAsync(item => item.Id == request.Id && !item.Eliminado, cancellationToken)
            ?? throw new InvalidOperationException("Traspaso de inventario no encontrado.");

        return new Response<TraspasoInventarioDTO>(_mapper.Map<TraspasoInventarioDTO>(traspaso));
    }
}
