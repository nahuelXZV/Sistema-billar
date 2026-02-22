using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.ListaPrecio.Commands;

public class UpdateListaPreciosCommand : ICommand<Response<bool>>
{
    public required ListaPrecioDTO ListaPrecioDTO { get; set; }
}

public class UpdateListaPreciosHandler : ICommandHandler<UpdateListaPreciosCommand, Response<bool>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IRepository<ListaPrecios> _repository;
    private readonly IRepository<ListaPreciosDetalle> _rpDetalles;

    public UpdateListaPreciosHandler(IMediator mediator, IMapper mapper, IRepository<ListaPrecios> repository, IRepository<ListaPreciosDetalle> rpDetalles)
    {
        _mediator = mediator;
        _mapper = mapper;
        _repository = repository;
        _rpDetalles = rpDetalles;
    }

    public async Task<Response<bool>> Handle(UpdateListaPreciosCommand request, CancellationToken cancellationToken)
    {
        var listaPrecio = await _repository.GetByIdAsync(request.ListaPrecioDTO.Id);
        if (listaPrecio == null) throw new ArgumentException("La lista de precios no existe.");

        //var detalles = listaPrecio.ListaDetalles;
        var detallesSaved = await _rpDetalles.Query().Where(x => x.IdListaPrecio == request.ListaPrecioDTO.Id).ToListAsync();
        _rpDetalles.DeleteRange(detallesSaved);
        await _rpDetalles.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        //listaPrecio.ListaDetalles = null;

        _repository.Attach(listaPrecio);
        _mapper.Map(request.ListaPrecioDTO, listaPrecio);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);


        //foreach (var item in detalles)
        //{
        //    item.Id = 0;
        //}
        //var detallesToAdd = _mapper.Map<List<ListaPreciosDetalle>>(request.ListaPrecioDTO.ListaDetalles);
        //await _rpDetalles.AddRangeAsync(detallesToAdd);


        return new Response<bool>(true);
    }
}