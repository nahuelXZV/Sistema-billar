using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.UnidadesMedidas.Queries;

public class GetUnidadMedidaByIdQuery : IQuery<Response<UnidadMedidaDTO>>
{
    public required long Id { get; set; }
}

public class GetUnidadMedidaByIdQueryHandler : IQueryHandler<GetUnidadMedidaByIdQuery, Response<UnidadMedidaDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<UnidadMedida> _repository;

    public GetUnidadMedidaByIdQueryHandler(IMapper mapper, IRepository<UnidadMedida> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<UnidadMedidaDTO>> Handle(GetUnidadMedidaByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.Query()
            .Where(p => !p.Eliminado)
            .Where(p => p.Id == request.Id);

        var unidad = await query.FirstOrDefaultAsync();
        if (unidad == null) throw new Exception("Unidad de medida no encontrado.");

        var unidadDto = _mapper.Map<UnidadMedidaDTO>(unidad);
        return new Response<UnidadMedidaDTO>(unidadDto);
    }
}

