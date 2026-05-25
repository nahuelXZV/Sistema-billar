using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using MediatR;

namespace Application.Features.Sales.Vendedores.Commands;

public class CreateVendedorCommand : ICommand<Response<long>>
{
    public required VendedorDTO VendedorDTO { get; set; }
}

public class CreateVendedorCommandHandler : ICommandHandler<CreateVendedorCommand, Response<long>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IRepository<Vendedor> _repository;

    public CreateVendedorCommandHandler(IMediator mediator, IMapper mapper, IRepository<Vendedor> repository)
    {
        _mediator = mediator;
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<long>> Handle(CreateVendedorCommand request, CancellationToken cancellationToken)
    {
        Vendedor vendedor = _mapper.Map<Vendedor>(request.VendedorDTO);
        vendedor = await _repository.AddAsync(vendedor);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<long>(vendedor.Id);
    }
}
