using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.Productos.Queries;

public class GetProductoByIdVendedorQuery : ICommand<Response<ProductoDTO>>
{
    public long IdProducto { get; set; }
    public long IdVendedor { get; set; }
}

public class GetProductoByIdVendedorQueryHandler : ICommandHandler<GetProductoByIdVendedorQuery, Response<ProductoDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Producto> _repository;

    public GetProductoByIdVendedorQueryHandler(IMapper mapper, IRepository<Producto> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<ProductoDTO>> Handle(
        GetProductoByIdVendedorQuery request,
        CancellationToken cancellationToken)
    {
        var producto = await _repository.Query()
            .Where(p => !p.Eliminado && p.Id == request.IdProducto)
            .FirstOrDefaultAsync(cancellationToken);

        if (producto is null)
        {
            throw new Exception("Producto no encontrado.");
        }

        var productoDto = _mapper.Map<ProductoDTO>(producto);

        var idListaPrecio = await _repository.Query<Vendedor>()
            .Where(v => !v.Eliminado && v.Activo && v.Id == request.IdVendedor)
            .Select(v => v.IdListaPrecio)
            .FirstOrDefaultAsync(cancellationToken);

        if (idListaPrecio <= 0)
        {
            return new Response<ProductoDTO>(productoDto);
        }

        var precio = await _repository.Query<ListaPreciosDetalle>()
            .Where(detalle => !detalle.Eliminado
                && detalle.IdListaPrecio == idListaPrecio
                && detalle.IdProducto == request.IdProducto)
            .Select(detalle => (decimal?)detalle.Precio)
            .FirstOrDefaultAsync(cancellationToken);

        productoDto.Precio = precio ?? 0m;
        return new Response<ProductoDTO>(productoDto);
    }
}
