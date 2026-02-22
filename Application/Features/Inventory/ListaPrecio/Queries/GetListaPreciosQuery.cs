using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace Application.Features.Inventory.ListaPrecio.Queries;

public class GetListaPreciosQuery : ICommand<Response<List<ListaPrecioDTO>>>
{
}

public class GetListaPreciosHandler : ICommandHandler<GetListaPreciosQuery, Response<List<ListaPrecioDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<ListaPrecios> _repository;

    public GetListaPreciosHandler(IMapper mapper, IRepository<ListaPrecios> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<List<ListaPrecioDTO>>> Handle(GetListaPreciosQuery request, CancellationToken cancellationToken)
    {
        var listaPrecios = await _repository.Query().Where(p => !p.Eliminado).ToListAsync(cancellationToken);
        var listaPreciosDtos = _mapper.Map<List<ListaPrecioDTO>>(listaPrecios);
        return new Response<List<ListaPrecioDTO>>(listaPreciosDtos);
    }
}
