using Domain.Validators.Inventory;
using FluentValidation;

namespace Application.Features.Inventory.Categorias.Commands.Validators;

public class CreateCategoriaCommandValidator : AbstractValidator<CreateCategoriaCommand>
{
    public CreateCategoriaCommandValidator()
    {
        RuleFor(command => command.CategoriaDTO)
            .SetValidator(new CreateCategoriaDTOValidator());

    }
}

