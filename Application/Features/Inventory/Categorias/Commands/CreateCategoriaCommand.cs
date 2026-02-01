using Application.Interfaces;
using AutoMapper;
using Domain.Common;
using Domain.DTOs.Inventory;
using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using MediatR;

namespace Application.Features.Inventory.Categorias.Commands;

public class CreateCategoriaCommand : ICommand<Response<long>>
{
    public required CategoriaDTO CategoriaDTO { get; set; }
}

public class CreateCategoriaHandler : ICommandHandler<CreateCategoriaCommand, Response<long>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IRepository<Categoria> _repository;

    public CreateCategoriaHandler(IMediator mediator, IMapper mapper, IRepository<Categoria> repository)
    {
        _mediator = mediator;
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<Response<long>> Handle(CreateCategoriaCommand request, CancellationToken cancellationToken)
    {
        Categoria categoria = _mapper.Map<Categoria>(request.CategoriaDTO);
        categoria = await _repository.AddAsync(categoria);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<long>(categoria.Id);
    }
}
