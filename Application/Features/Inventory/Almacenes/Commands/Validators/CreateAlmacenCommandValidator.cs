using Domain.Validators.Inventory;
using FluentValidation;

namespace Application.Features.Inventory.Almacenes.Commands.Validators;

public class CreateAlmacenCommandValidator : AbstractValidator<CreateAlmacenCommand>
{
    public CreateAlmacenCommandValidator()
    {
        RuleFor(command => command.AlmacenDTO)
            .SetValidator(new CreateAlmacenDTOValidator());

    }
}

