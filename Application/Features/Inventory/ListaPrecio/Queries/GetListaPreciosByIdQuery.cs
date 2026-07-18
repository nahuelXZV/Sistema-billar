using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.ListaPrecio.Queries;

public class GetListaPreciosByIdQuery : IQuery<Response<ListaPrecioDTO>>
{
    public required long Id { get; set; }
}

public class GetListaPreciosByIdHandler : IQueryHandler<GetListaPreciosByIdQuery, Response<ListaPrecioDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<ListaPrecios> _repository;

    public GetListaPreciosByIdHandler(IMapper mapper, IRepository<ListaPrecios> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<ListaPrecioDTO>> Handle(GetListaPreciosByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.Query()
            .Include(p => p.ListaDetalles)
            .Where(p => p.Id == request.Id);

        var listado = await query.FirstOrDefaultAsync();
        if (listado == null) throw new Exception("Lista de precios no encontrado.");

        var listadoDtos = _mapper.Map<ListaPrecioDTO>(listado);
        return new Response<ListaPrecioDTO>(listadoDtos);
    }
}

