
using Domain.DTOs.Configuration;
using FluentValidation;

namespace Domain.Validators.Configuration;

public class CreateMesaDTOValidator : AbstractValidator<MesaDTO>
{
    public CreateMesaDTOValidator()
    {
        RuleFor(p => p.Nombre)
            .NotEmpty().WithMessage("{PropertyName} es requerido.")
            .NotNull()
            .MaximumLength(100).WithMessage("{PropertyName} no debe exceder los 100 caracteres.");

        RuleFor(p => p.IdTipoMesa)
            .GreaterThan(0).WithMessage("{PropertyName} debe ser mayor que 0.");
    }
}
