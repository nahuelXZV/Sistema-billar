using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Contact;
using Domain.Entities.Contact;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Contact.Proveedores.Queries;

public class GetProveedorByIdQuery : IQuery<Response<ProveedorDTO>>
{
    public long Id { get; set; }
}

public class GetProveedorByIdQueryHandler : IQueryHandler<GetProveedorByIdQuery, Response<ProveedorDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Proveedor> _repository;

    public GetProveedorByIdQueryHandler(IMapper mapper, IRepository<Proveedor> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<ProveedorDTO>> Handle(GetProveedorByIdQuery request, CancellationToken cancellationToken)
    {
        var proveedor = await _repository.Query()
            .Where(item => item.Id == request.Id && !item.Eliminado)
            .Include(item => item.ListaProductos.Where(costo => !costo.Eliminado))
                .ThenInclude(costo => costo.Producto)
            .Include(item => item.ListaProductos.Where(costo => !costo.Eliminado))
                .ThenInclude(costo => costo.ProductoConversion)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ArgumentException("Proveedor no encontrado.");

        return new Response<ProveedorDTO>(_mapper.Map<ProveedorDTO>(proveedor));
    }
}
