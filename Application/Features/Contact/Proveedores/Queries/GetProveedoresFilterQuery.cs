using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Contact;
using Domain.DTOs.Shared;
using Domain.Entities.Contact;
using Domain.Extensions;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Contact.Proveedores.Queries;

public class GetProveedoresFilterQuery : IQuery<Response<ResponseFilterDTO<ProveedorDTO>>>
{
    public FilterDTO? Filter { get; set; }
}

public class GetProveedoresFilterQueryHandler : IQueryHandler<GetProveedoresFilterQuery, Response<ResponseFilterDTO<ProveedorDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Proveedor> _repository;

    public GetProveedoresFilterQueryHandler(IMapper mapper, IRepository<Proveedor> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<ResponseFilterDTO<ProveedorDTO>>> Handle(GetProveedoresFilterQuery request, CancellationToken cancellationToken)
    {
        var baseQuery = _repository.Query()
            .Where(proveedor => !proveedor.Eliminado);

        var total = await baseQuery.CountAsync(cancellationToken);
        var search = request.Filter?.Search?.Trim().ToLower();

        var query = baseQuery
            .Include(proveedor => proveedor.ListaProductos.Where(costo => !costo.Eliminado))
                .ThenInclude(costo => costo.Producto)
            .Include(proveedor => proveedor.ListaProductos.Where(costo => !costo.Eliminado))
                .ThenInclude(costo => costo.ProductoConversion)
            .ApplyFilter(
                request.Filter,
                proveedor => string.IsNullOrEmpty(search)
                    || (proveedor.NombreComercial != null && proveedor.NombreComercial.ToLower().Contains(search))
                    || (proveedor.NombreContacto != null && proveedor.NombreContacto.ToLower().Contains(search))
                    || (proveedor.Telefono != null && proveedor.Telefono.ToLower().Contains(search)));

        var proveedores = await query.ToListAsync(cancellationToken);
        var response = new ResponseFilterDTO<ProveedorDTO>
        {
            Data = _mapper.Map<List<ProveedorDTO>>(proveedores),
            Total = total
        };

        return new Response<ResponseFilterDTO<ProveedorDTO>>(response);
    }
}
