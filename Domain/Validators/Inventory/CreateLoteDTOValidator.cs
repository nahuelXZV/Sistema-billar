using Domain.DTOs.Inventory;
using FluentValidation;

namespace Domain.Validators.Inventory;

public class CreateLoteDTOValidator : AbstractValidator<LoteDTO>
{
    public CreateLoteDTOValidator()
    {
        RuleFor(p => p.Codigo)
            .NotEmpty().WithMessage("{PropertyName} es requerido.")
            .NotNull()
            .MaximumLength(100).WithMessage("{PropertyName} no debe exceder los 100 caracteres.");

        RuleFor(p => p.FechaVencimiento)
            .NotEmpty().WithMessage("{PropertyName} es requerido.")
            .NotNull();

        RuleFor(p => p.FechaFabricacion)
                .NotEmpty().WithMessage("{PropertyName} es requerido.")
                .NotNull();

    }
}
