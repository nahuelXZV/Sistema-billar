using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sales.Vendedores.Queries;

public class GetVendedorByIdQuery : ICommand<Response<VendedorDTO>>
{
    public required long Id { get; set; }
}

public class GetVendedorByIdQueryHandler : ICommandHandler<GetVendedorByIdQuery, Response<VendedorDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Vendedor> _repository;

    public GetVendedorByIdQueryHandler(IMapper mapper, IRepository<Vendedor> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<VendedorDTO>> Handle(GetVendedorByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.Query()
            .Where(p => !p.Eliminado)
            .Where(p => p.Id == request.Id)
            .Include(p => p.Usuario);

        var vendedor = await query.FirstOrDefaultAsync(cancellationToken);
        if (vendedor == null) throw new Exception("Vendedor no encontrado.");

        var vendedorDTO = _mapper.Map<VendedorDTO>(vendedor);
        return new Response<VendedorDTO>(vendedorDTO);
    }
}
