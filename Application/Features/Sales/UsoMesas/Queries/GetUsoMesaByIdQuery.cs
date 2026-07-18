using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sales.UsoMesas.Queries;

public class GetUsoMesaByIdQuery : IQuery<Response<UsoMesaDTO>>
{
    public required long Id { get; set; }
}

public class GetUsoMesaByIdQueryHandler : IQueryHandler<GetUsoMesaByIdQuery, Response<UsoMesaDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<UsoMesa> _repository;

    public GetUsoMesaByIdQueryHandler(IMapper mapper, IRepository<UsoMesa> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<UsoMesaDTO>> Handle(GetUsoMesaByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.Query()
            .Where(p => !p.Eliminado)
            .Where(p => p.Id == request.Id)
            .Include(p => p.Mesa);

        var usoMesa = await query.FirstOrDefaultAsync(cancellationToken);
        if (usoMesa == null) throw new Exception("Uso de mesa no encontrado.");

        var usoMesaDto = _mapper.Map<UsoMesaDTO>(usoMesa);
        return new Response<UsoMesaDTO>(usoMesaDto);
    }
}
