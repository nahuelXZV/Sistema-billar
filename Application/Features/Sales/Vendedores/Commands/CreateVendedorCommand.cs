using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;

namespace Application.Features.Sales.Vendedores.Commands;

public class CreateVendedorCommand : ICommand<Response<long>>
{
    public required VendedorDTO VendedorDTO { get; set; }
}

public class CreateVendedorCommandHandler : ICommandHandler<CreateVendedorCommand, Response<long>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Vendedor> _repository;
    private readonly IRepository<VendedorAlmacenes> _vendedorAlmacenRepository;

    public CreateVendedorCommandHandler(
        IMapper mapper,
        IRepository<Vendedor> repository,
        IRepository<VendedorAlmacenes> vendedorAlmacenRepository)
    {
        _mapper = mapper;
        _repository = repository;
        _vendedorAlmacenRepository = vendedorAlmacenRepository;
    }

    public async Task<Response<long>> Handle(CreateVendedorCommand request, CancellationToken cancellationToken)
    {
        Vendedor vendedor = _mapper.Map<Vendedor>(request.VendedorDTO);
        vendedor.ListaAlmacenes = new();

        vendedor = await _repository.AddAsync(vendedor);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        var almacenesSeleccionados = request.VendedorDTO.ListaAlmacenes
            .Where(p => p.IdAlmacen > 0)
            .GroupBy(p => p.IdAlmacen)
            .Select(p => p.First())
            .Select(p => new VendedorAlmacenes
            {
                IdVendedor = vendedor.Id,
                IdAlmacen = p.IdAlmacen
            })
            .ToList();

        if (almacenesSeleccionados.Count > 0)
        {
            await _vendedorAlmacenRepository.AddRangeAsync(almacenesSeleccionados);
            await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        }

        return new Response<long>(vendedor.Id);
    }
}
