using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;

namespace Application.Features.Sales.Ventas.Commands;

public class CreateVentaCommand : ICommand<Response<long>>
{
    public required VentaDTO VentaDTO { get; set; }
}

public class CreateVentaCommandHandler : ICommandHandler<CreateVentaCommand, Response<long>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Venta> _repository;

    public CreateVentaCommandHandler(IMapper mapper, IRepository<Venta> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<long>> Handle(CreateVentaCommand request, CancellationToken cancellationToken)
    {
        Venta venta = _mapper.Map<Venta>(request.VentaDTO);
        venta = await _repository.AddAsync(venta);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<long>(venta.Id);
    }
}
