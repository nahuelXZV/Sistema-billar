using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;

namespace Application.Features.Sales.UsoMesas.Commands;

public class CreateUsoMesaCommand : ICommand<Response<long>>
{
    public required UsoMesaDTO UsoMesaDTO { get; set; }
}

public class CreateUsoMesaCommandHandler : ICommandHandler<CreateUsoMesaCommand, Response<long>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<UsoMesa> _repository;

    public CreateUsoMesaCommandHandler(IMapper mapper, IRepository<UsoMesa> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<long>> Handle(CreateUsoMesaCommand request, CancellationToken cancellationToken)
    {
        UsoMesa usoMesa = _mapper.Map<UsoMesa>(request.UsoMesaDTO);
        usoMesa = await _repository.AddAsync(usoMesa);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<long>(usoMesa.Id);
    }
}
