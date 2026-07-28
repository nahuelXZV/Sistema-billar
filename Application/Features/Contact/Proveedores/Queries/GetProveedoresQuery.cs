using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Contact;
using Domain.Entities.Contact;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Contact.Proveedores.Queries;

public class GetProveedoresQuery : IQuery<Response<List<ProveedorDTO>>>
{
}

public class GetProveedoresQueryHandler : IQueryHandler<GetProveedoresQuery, Response<List<ProveedorDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Proveedor> _repository;

    public GetProveedoresQueryHandler(IMapper mapper, IRepository<Proveedor> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<List<ProveedorDTO>>> Handle(GetProveedoresQuery request, CancellationToken cancellationToken)
    {
        var proveedores = await _repository.Query()
            .Where(proveedor => !proveedor.Eliminado)
            .Include(proveedor => proveedor.ListaProductos.Where(costo => !costo.Eliminado))
                .ThenInclude(costo => costo.Producto)
            .Include(proveedor => proveedor.ListaProductos.Where(costo => !costo.Eliminado))
                .ThenInclude(costo => costo.ProductoConversion)
            .ToListAsync(cancellationToken);

        return new Response<List<ProveedorDTO>>(_mapper.Map<List<ProveedorDTO>>(proveedores));
    }
}
