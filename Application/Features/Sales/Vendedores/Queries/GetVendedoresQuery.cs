using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sales.Vendedores.Queries;

public class GetVendedoresQuery : ICommand<Response<List<VendedorDTO>>>
{
}

public class GetVendedoresQueryHandler : ICommandHandler<GetVendedoresQuery, Response<List<VendedorDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Vendedor> _repository;

    public GetVendedoresQueryHandler(IMapper mapper, IRepository<Vendedor> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<List<VendedorDTO>>> Handle(GetVendedoresQuery request, CancellationToken cancellationToken)
    {
        var listaVendedores = await _repository.Query()
            .Include(p => p.Usuario)
            .Include(p => p.ListaPrecio)
            .Include(p => p.ListaAlmacenes.Where(a => !a.Eliminado))
            .ThenInclude(a => a.Almacen)
            .Where(p => !p.Eliminado)
            .ToListAsync(cancellationToken);

        var listaVendedoresDTO = _mapper.Map<List<VendedorDTO>>(listaVendedores);
        return new Response<List<VendedorDTO>>(listaVendedoresDTO);
    }
}
