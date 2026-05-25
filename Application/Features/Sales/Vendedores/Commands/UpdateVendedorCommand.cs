using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sales.Vendedores.Commands;

public class UpdateVendedorCommand : ICommand<Response<bool>>
{
    public required VendedorDTO VendedorDTO { get; set; }
}

public class UpdateVendedorCommandHandler : ICommandHandler<UpdateVendedorCommand, Response<bool>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Vendedor> _repository;
    private readonly IRepository<VendedorAlmacenes> _vendedorAlmacenRepository;

    public UpdateVendedorCommandHandler(
        IMapper mapper,
        IRepository<Vendedor> repository,
        IRepository<VendedorAlmacenes> vendedorAlmacenRepository)
    {
        _mapper = mapper;
        _repository = repository;
        _vendedorAlmacenRepository = vendedorAlmacenRepository;
    }

    public async Task<Response<bool>> Handle(UpdateVendedorCommand request, CancellationToken cancellationToken)
    {
        var vendedor = await _repository.GetByIdAsync(request.VendedorDTO.Id);
        if (vendedor == null) throw new ArgumentException("El vendedor no existe.");

        _repository.Update(vendedor);
        _mapper.Map(request.VendedorDTO, vendedor);

        var almacenesSeleccionados = request.VendedorDTO.ListaAlmacenes
            .Where(p => p.IdAlmacen > 0)
            .Select(p => p.IdAlmacen)
            .Distinct()
            .ToHashSet();

        var relacionesActuales = await _vendedorAlmacenRepository.Query()
            .Where(p => p.IdVendedor == request.VendedorDTO.Id)
            .ToListAsync(cancellationToken);

        foreach (var relacion in relacionesActuales)
        {
            var seleccionado = almacenesSeleccionados.Contains(relacion.IdAlmacen);

            if (seleccionado && relacion.Eliminado)
            {
                _vendedorAlmacenRepository.Update(relacion);
                relacion.Eliminado = false;
            }
            else if (!seleccionado && !relacion.Eliminado)
            {
                _vendedorAlmacenRepository.Delete(relacion);
            }
        }

        var nuevosAlmacenes = almacenesSeleccionados
            .Where(idAlmacen => !relacionesActuales.Any(p => p.IdAlmacen == idAlmacen))
            .Select(idAlmacen => new VendedorAlmacenes
            {
                IdVendedor = request.VendedorDTO.Id,
                IdAlmacen = idAlmacen
            })
            .ToList();

        if (nuevosAlmacenes.Count > 0)
        {
            await _vendedorAlmacenRepository.AddRangeAsync(nuevosAlmacenes);
        }

        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}
