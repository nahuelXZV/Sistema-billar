using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Configuration;
using Domain.Entities.Configuration;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Configuration.Mesas.Queries;

public class GetMesaByIdQuery : IQuery<Response<MesaDTO>>
{
    public required long Id { get; set; }
}

public class GetMesaByIdQueryHandler : IQueryHandler<GetMesaByIdQuery, Response<MesaDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Mesa> _repository;

    public GetMesaByIdQueryHandler(IMapper mapper, IRepository<Mesa> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<MesaDTO>> Handle(GetMesaByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.Query()
            .Include(p => p.TipoMesa)
            .Where(p => !p.Eliminado)
            .Where(p => p.Id == request.Id);

        var mesa = await query.FirstOrDefaultAsync(cancellationToken);
        if (mesa == null) throw new Exception("Mesa no encontrada.");

        var mesaDto = _mapper.Map<MesaDTO>(mesa);
        return new Response<MesaDTO>(mesaDto);
    }
}
