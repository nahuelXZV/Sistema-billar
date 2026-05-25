using Domain.DTOs.Contact;
using FluentValidation;

namespace Domain.Validators.Contact;

public class CreateClienteDTOValidator : AbstractValidator<ClienteDTO>
{
    public CreateClienteDTOValidator()
    {
        RuleFor(p => p.Nombre)
            .NotEmpty().WithMessage("{PropertyName} es requerido.")
            .NotNull()
            .MaximumLength(250).WithMessage("{PropertyName} no debe exceder los 250 caracteres.");

        RuleFor(p => p.Documento)
            .NotEmpty().WithMessage("{PropertyName} es requerido.")
            .NotNull()
            .MaximumLength(100).WithMessage("{PropertyName} no debe exceder los 100 caracteres.");

        RuleFor(p => p.Telefono)
            .MaximumLength(50).WithMessage("{PropertyName} no debe exceder los 50 caracteres.");
    }
}
