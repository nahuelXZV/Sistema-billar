using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sales.Vendedores.Queries;

public class GetVendedorByUsuarioQuery : IQuery<Response<VendedorDTO>>
{
    public long IdUsuario { get; set; }
}

public class GetVendedorByUsuarioQueryHandler : IQueryHandler<GetVendedorByUsuarioQuery, Response<VendedorDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Vendedor> _repository;

    public GetVendedorByUsuarioQueryHandler(IMapper mapper, IRepository<Vendedor> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<VendedorDTO>> Handle(GetVendedorByUsuarioQuery request, CancellationToken cancellationToken)
    {
        var vendedor = await _repository.Query()
            .Where(v => !v.Eliminado && v.Activo && v.IdUsuario == request.IdUsuario)
            .Include(v => v.Usuario)
            .Include(v => v.ListaPrecio)
            .Include(v => v.ListaAlmacenes.Where(a => !a.Eliminado))
            .ThenInclude(a => a.Almacen)
            .FirstOrDefaultAsync(cancellationToken);

        return new Response<VendedorDTO>(vendedor is null ? new VendedorDTO() : _mapper.Map<VendedorDTO>(vendedor));
    }
}
