using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Purchases;
using Domain.Entities.Purchases;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Purchases.Compras.Queries;

public class GetCompraByIdQuery : IQuery<Response<CompraDTO>>
{
    public long Id { get; set; }
}

public class GetCompraByIdQueryHandler : IQueryHandler<GetCompraByIdQuery, Response<CompraDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Compra> _repository;

    public GetCompraByIdQueryHandler(IMapper mapper, IRepository<Compra> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<CompraDTO>> Handle(GetCompraByIdQuery request, CancellationToken cancellationToken)
    {
        var compra = await GetComprasQueryHandler.IncluirRelaciones(_repository.Query())
            .FirstOrDefaultAsync(item => item.Id == request.Id && !item.Eliminado, cancellationToken)
            ?? throw new ArgumentException("Compra no encontrada.");

        return new Response<CompraDTO>(_mapper.Map<CompraDTO>(compra));
    }
}
