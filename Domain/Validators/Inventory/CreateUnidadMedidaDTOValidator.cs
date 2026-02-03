using Domain.DTOs.Inventory;
using FluentValidation;

namespace Domain.Validators.Inventory;

public class CreateUnidadMedidaDTOValidator : AbstractValidator<UnidadMedidaDTO>
{
    public CreateUnidadMedidaDTOValidator()
    {
        RuleFor(p => p.Nombre)
            .NotEmpty().WithMessage("{PropertyName} es requerido.")
            .NotNull()
            .MaximumLength(250).WithMessage("{PropertyName} no debe exceder los 250 caracteres.");

        RuleFor(p => p.Abreviatura)
            .MaximumLength(20).WithMessage("{PropertyName} no debe exceder los 20 caracteres.");

        RuleFor(p => p.Tipo)
            .NotEmpty().WithMessage("{PropertyName} es requerido.")
            .NotNull();
    }
}

