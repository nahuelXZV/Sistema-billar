using Domain.DTOs.Sales;
using FluentValidation;

namespace Domain.Validators.Sales;

public class CreateMetodoPagoDTOValidator : AbstractValidator<MetodoPagoDTO>
{
    public CreateMetodoPagoDTOValidator()
    {
        RuleFor(p => p.Nombre)
            .NotEmpty().WithMessage("{PropertyName} es requerido.")
            .NotNull()
            .MaximumLength(250).WithMessage("{PropertyName} no debe exceder los 250 caracteres.");

        RuleFor(p => p.Abreviatura)
            .NotEmpty().WithMessage("{PropertyName} es requerido.")
            .NotNull()
            .MaximumLength(20).WithMessage("{PropertyName} no debe exceder los 20 caracteres.");

        RuleFor(p => p.ClaveMoneda)
            .NotEmpty().WithMessage("{PropertyName} es requerido.")
            .NotNull()
            .MaximumLength(10).WithMessage("{PropertyName} no debe exceder los 10 caracteres.");

        RuleFor(p => p.Icono)
            .MaximumLength(100).WithMessage("{PropertyName} no debe exceder los 100 caracteres.");
    }
}
