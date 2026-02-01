using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.Categorias.Queries;

public class GetCategoriaByIdQuery : ICommand<Response<CategoriaDTO>>
{
    public required long Id { get; set; }
}

public class GetCategoriaByIdHandler : ICommandHandler<GetCategoriaByIdQuery, Response<CategoriaDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Categoria> _repository;

    public GetCategoriaByIdHandler(IMapper mapper, IRepository<Categoria> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<CategoriaDTO>> Handle(GetCategoriaByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.Query()
            .Where(p => !p.Eliminado)
            .Where(p => p.Id == request.Id);

        var categoria = await query.FirstOrDefaultAsync();
        if (categoria == null) throw new Exception("Categoria no encontrado.");

        var categoriaDto = _mapper.Map<CategoriaDTO>(categoria);
        return new Response<CategoriaDTO>(categoriaDto);
    }
}