using Domain.Validators.Inventory;
using FluentValidation;

namespace Application.Features.Inventory.Productos.Commands.Validators;

public class CreateProductoCommandValidator : AbstractValidator<CreateProductoCommand>
{
    public CreateProductoCommandValidator()
    {
        RuleFor(command => command.ProductoDTO)
        .SetValidator(new CreateProductoDTOValidator());
    }
}


