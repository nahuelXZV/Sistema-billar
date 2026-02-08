using Domain.Validators.Inventory;
using FluentValidation;

namespace Application.Features.Inventory.UnidadesMedidas.Commands.Validators;

public class CreateUnidadMedidaValidator : AbstractValidator<CreateUnidadMedidaCommand>
{
    public CreateUnidadMedidaValidator()
    {
        RuleFor(command => command.UnidadMedidaDTO)
            .SetValidator(new CreateUnidadMedidaDTOValidator());
    }
}

