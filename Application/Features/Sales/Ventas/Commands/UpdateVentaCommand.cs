using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;

namespace Application.Features.Sales.Ventas.Commands;

public class UpdateVentaCommand : ICommand<Response<bool>>
{
    public required VentaDTO VentaDTO { get; set; }
}

public class UpdateVentaCommandHandler : ICommandHandler<UpdateVentaCommand, Response<bool>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Venta> _repository;

    public UpdateVentaCommandHandler(IMapper mapper, IRepository<Venta> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<bool>> Handle(UpdateVentaCommand request, CancellationToken cancellationToken)
    {
        var venta = await _repository.GetByIdAsync(request.VentaDTO.Id);
        if (venta == null) throw new ArgumentException("La venta no existe.");

        _repository.Update(venta);
        _mapper.Map(request.VentaDTO, venta);

        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}
