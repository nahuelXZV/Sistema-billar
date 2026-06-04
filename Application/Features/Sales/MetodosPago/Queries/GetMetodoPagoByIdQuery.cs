using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sales.MetodosPago.Queries;

public class GetMetodoPagoByIdQuery : ICommand<Response<MetodoPagoDTO>>
{
    public required long Id { get; set; }
}

public class GetMetodoPagoByIdQueryHandler : ICommandHandler<GetMetodoPagoByIdQuery, Response<MetodoPagoDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<MetodoPago> _repository;

    public GetMetodoPagoByIdQueryHandler(IMapper mapper, IRepository<MetodoPago> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<MetodoPagoDTO>> Handle(GetMetodoPagoByIdQuery request, CancellationToken cancellationToken)
    {
        var metodoPago = await _repository.Query()
            .Where(p => !p.Eliminado)
            .Where(p => p.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (metodoPago == null) throw new Exception("Metodo de pago no encontrado.");

        var metodoPagoDto = _mapper.Map<MetodoPagoDTO>(metodoPago);
        return new Response<MetodoPagoDTO>(metodoPagoDto);
    }
}
