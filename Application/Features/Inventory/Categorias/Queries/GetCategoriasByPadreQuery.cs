using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.Categorias.Queries;

public class GetCategoriasByPadreQuery : IQuery<Response<List<CategoriaDTO>>>
{
    public long? IdCategoriaPadre { get; set; }
}

public class GetCategoriasByPadreHandler : IQueryHandler<GetCategoriasByPadreQuery, Response<List<CategoriaDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Categoria> _repository;

    public GetCategoriasByPadreHandler(IMapper mapper, IRepository<Categoria> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<List<CategoriaDTO>>> Handle(GetCategoriasByPadreQuery request, CancellationToken cancellationToken)
    {
        var categorias = await _repository.Query()
            .Where(c => !c.Eliminado && c.Activo && c.IdCategoriaPadre == request.IdCategoriaPadre)
            .OrderBy(c => c.OrdenVisual)
            .ToListAsync(cancellationToken);

        var categoriasDto = _mapper.Map<List<CategoriaDTO>>(categorias);
        return new Response<List<CategoriaDTO>>(categoriasDto);
    }
}
