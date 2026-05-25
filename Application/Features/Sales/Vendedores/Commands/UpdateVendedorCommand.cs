using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using MediatR;

namespace Application.Features.Sales.Vendedores.Commands;

public class UpdateVendedorCommand : ICommand<Response<bool>>
{
    public required VendedorDTO VendedorDTO { get; set; }
}

public class UpdateVendedorCommandHandler : ICommandHandler<UpdateVendedorCommand, Response<bool>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IRepository<Vendedor> _repository;

    public UpdateVendedorCommandHandler(IMediator mediator, IMapper mapper, IRepository<Vendedor> repository)
    {
        _mediator = mediator;
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<bool>> Handle(UpdateVendedorCommand request, CancellationToken cancellationToken)
    {
        var vendedor = await _repository.GetByIdAsync(request.VendedorDTO.Id);
        if (vendedor == null) throw new ArgumentException("El vendedor no existe.");

        _repository.Update(vendedor);
        _mapper.Map(request.VendedorDTO, vendedor);

        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}
