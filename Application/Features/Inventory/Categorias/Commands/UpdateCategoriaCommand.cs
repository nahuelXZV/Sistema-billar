using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using MediatR;

namespace Application.Features.Inventory.Categorias.Commands;

public class UpdateCategoriaCommand : ICommand<Response<bool>>
{
    public CategoriaDTO CategoriaDTO { get; set; }
}

public class UpdateCategoriaHandler : ICommandHandler<UpdateCategoriaCommand, Response<bool>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IRepository<Categoria> _repository;

    public UpdateCategoriaHandler(IMediator mediator, IMapper mapper, IRepository<Categoria> repository)
    {
        _mediator = mediator;
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<bool>> Handle(UpdateCategoriaCommand request, CancellationToken cancellationToken)
    {
        var categoria = await _repository.GetByIdAsync(request.CategoriaDTO.Id);
        if (categoria == null) throw new ArgumentException("La categoria no existe.");

        _repository.Update(categoria);
        _mapper.Map(request.CategoriaDTO, categoria);

        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}

