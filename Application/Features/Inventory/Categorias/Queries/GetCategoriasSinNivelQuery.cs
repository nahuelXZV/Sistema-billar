using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.Categorias.Queries;

public class GetCategoriasSinNivelQuery : ICommand<Response<List<CategoriaDTO>>>
{
}

public class GetCategoriasSinNivelHandler : ICommandHandler<GetCategoriasSinNivelQuery, Response<List<CategoriaDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Categoria> _repository;

    public GetCategoriasSinNivelHandler(IMapper mapper, IRepository<Categoria> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<List<CategoriaDTO>>> Handle(GetCategoriasSinNivelQuery request, CancellationToken cancellationToken)
    {
        var listaCategorias = await _repository.Query().Where(p => !p.Eliminado).OrderBy(p => p.OrdenVisual)
            .ToListAsync(cancellationToken);

        var listaCategoriasDtos = _mapper.Map<List<CategoriaDTO>>(listaCategorias);
        return new Response<List<CategoriaDTO>>(listaCategoriasDtos);
    }

}

