using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.Entities.Configuration;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Configuration.TipoMesas.Queries;

public class VerificarMesaByIdProductoQuery : IQuery<Response<bool>>
{
    public required long IdProducto { get; set; }
}

public class VerificarMesaByIdProductoQueryHandler : IQueryHandler<VerificarMesaByIdProductoQuery, Response<bool>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<TipoMesa> _repository;

    public VerificarMesaByIdProductoQueryHandler(IMapper mapper, IRepository<TipoMesa> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<bool>> Handle(VerificarMesaByIdProductoQuery request, CancellationToken cancellationToken)
    {
        var tipo = await _repository.Query()
            .Where(p => !p.Eliminado)
            .Where(p => p.IdProducto == request.IdProducto)
            .FirstOrDefaultAsync(cancellationToken);

        if (tipo == null) return new Response<bool>(false);

        return new Response<bool>(true);
    }
}
