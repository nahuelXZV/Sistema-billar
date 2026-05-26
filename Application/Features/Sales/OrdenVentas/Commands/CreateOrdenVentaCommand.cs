using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;

namespace Application.Features.Sales.OrdenVentas.Commands;

public class CreateOrdenVentaCommand : ICommand<Response<long>>
{
    public required OrdenVentaDTO OrdenVentaDTO { get; set; }
}

public class CreateOrdenVentaCommandHandler : ICommandHandler<CreateOrdenVentaCommand, Response<long>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<OrdenVenta> _repository;

    public CreateOrdenVentaCommandHandler(IMapper mapper, IRepository<OrdenVenta> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<long>> Handle(CreateOrdenVentaCommand request, CancellationToken cancellationToken)
    {
        OrdenVenta ordenVenta = _mapper.Map<OrdenVenta>(request.OrdenVentaDTO);
        ordenVenta = await _repository.AddAsync(ordenVenta);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<long>(ordenVenta.Id);
    }
}
