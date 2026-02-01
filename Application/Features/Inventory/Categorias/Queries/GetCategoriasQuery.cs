using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.Categorias.Queries;

public class GetCategoriasQuery : ICommand<Response<List<CategoriaDTO>>>
{
    public FilterDTO? Filter { get; set; }
}

public class GetCategoriasHandler : ICommandHandler<GetCategoriasQuery, Response<List<CategoriaDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Categoria> _repository;
    private List<CategoriaDTO> ListaCategorias;

    public GetCategoriasHandler(IMapper mapper, IRepository<Categoria> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<List<CategoriaDTO>>> Handle(GetCategoriasQuery request, CancellationToken cancellationToken)
    {
        var listaCategorias = await _repository.Query().Where(p => !p.Eliminado).OrderBy(p => p.OrdenVisual).ToListAsync(cancellationToken);

        var listaCategoriasDtos = _mapper.Map<List<CategoriaDTO>>(listaCategorias);
        ListaCategorias = listaCategoriasDtos;

        var categoriasBases = listaCategoriasDtos.Where(c => c.IdCategoriaPadre == null).ToList();

        foreach (var categoriaBase in categoriasBases)
        {
            categoriaBase.SubCategorias = CargarSubCategorias(categoriaBase.Id);
        }

        return new Response<List<CategoriaDTO>>(categoriasBases);
    }

    private List<CategoriaDTO> CargarSubCategorias(long idCategoriaPadre)
    {
        var subCategorias = ListaCategorias
            .Where(c => c.IdCategoriaPadre == idCategoriaPadre)
            .ToList();

        foreach (var subCategoria in subCategorias)
        {
            subCategoria.SubCategorias = CargarSubCategorias(subCategoria.Id);
        }

        return subCategorias;
    }
}
