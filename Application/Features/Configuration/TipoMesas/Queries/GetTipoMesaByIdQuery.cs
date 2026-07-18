using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Configuration;
using Domain.Entities.Configuration;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Configuration.TipoMesas.Queries;

public class GetTipoMesaByIdQuery : IQuery<Response<TipoMesaDTO>>
{
    public required long Id { get; set; }
}

public class GetTipoMesaByIdQueryHandler : IQueryHandler<GetTipoMesaByIdQuery, Response<TipoMesaDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<TipoMesa> _repository;

    public GetTipoMesaByIdQueryHandler(IMapper mapper, IRepository<TipoMesa> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<TipoMesaDTO>> Handle(GetTipoMesaByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.Query()
            .Where(p => !p.Eliminado)
            .Where(p => p.Id == request.Id);

        var tipo = await query.FirstOrDefaultAsync();
        if (tipo == null) throw new Exception("Tipo de mesa no encontrado.");

        var tipoDto = _mapper.Map<TipoMesaDTO>(tipo);
        return new Response<TipoMesaDTO>(tipoDto);
    }
}
