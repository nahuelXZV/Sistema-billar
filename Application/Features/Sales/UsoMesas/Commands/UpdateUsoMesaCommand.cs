using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;

namespace Application.Features.Sales.UsoMesas.Commands;

public class UpdateUsoMesaCommand : ICommand<Response<bool>>
{
    public required UsoMesaDTO UsoMesaDTO { get; set; }
}

public class UpdateUsoMesaCommandHandler : ICommandHandler<UpdateUsoMesaCommand, Response<bool>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<UsoMesa> _repository;

    public UpdateUsoMesaCommandHandler(IMapper mapper, IRepository<UsoMesa> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<bool>> Handle(UpdateUsoMesaCommand request, CancellationToken cancellationToken)
    {
        var usoMesa = await _repository.GetByIdAsync(request.UsoMesaDTO.Id);
        if (usoMesa == null) throw new ArgumentException("El uso de mesa no existe.");

        _repository.Update(usoMesa);
        _mapper.Map(request.UsoMesaDTO, usoMesa);

        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}
