using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Domain.Interfaces.Shared;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.Almacenes.Queries;

public class GetAlmacenByIdQuery : ICommand<Response<AlmacenDTO>>
{
    public required long Id { get; set; }
}

public class GetAlmacenByIdHandler : ICommandHandler<GetAlmacenByIdQuery, Response<AlmacenDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<Almacen> _repository;

    public GetAlmacenByIdHandler(IMapper mapper, IRepository<Almacen> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<AlmacenDTO>> Handle(GetAlmacenByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.Query()
            .Where(p => !p.Eliminado)
            .Where(p => p.Id == request.Id);

        var almacen = await query.FirstOrDefaultAsync();
        if (almacen == null) throw new Exception("Usuario no encontrado.");

        var almacenDTO = _mapper.Map<AlmacenDTO>(almacen);
        return new Response<AlmacenDTO>(almacenDTO);
    }
}

