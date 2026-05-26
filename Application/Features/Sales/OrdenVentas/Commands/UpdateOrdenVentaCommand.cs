using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;

namespace Application.Features.Sales.OrdenVentas.Commands;

public class UpdateOrdenVentaCommand : ICommand<Response<bool>>
{
    public required OrdenVentaDTO OrdenVentaDTO { get; set; }
}

public class UpdateOrdenVentaCommandHandler : ICommandHandler<UpdateOrdenVentaCommand, Response<bool>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<OrdenVenta> _repository;

    public UpdateOrdenVentaCommandHandler(IMapper mapper, IRepository<OrdenVenta> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<bool>> Handle(UpdateOrdenVentaCommand request, CancellationToken cancellationToken)
    {
        var ordenVenta = await _repository.GetByIdAsync(request.OrdenVentaDTO.Id);
        if (ordenVenta == null) throw new ArgumentException("La orden de venta no existe.");

        _repository.Update(ordenVenta);
        _mapper.Map(request.OrdenVentaDTO, ordenVenta);

        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}
