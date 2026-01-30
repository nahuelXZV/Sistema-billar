using Domain.DTOs.Security;
using FluentValidation;

namespace Domain.Validators.Security;

public class CreateUsuarioDTOValidator : AbstractValidator<UsuarioDTO>
{
    public CreateUsuarioDTOValidator()
    {
        RuleFor(p => p.Username)
            .NotEmpty().WithMessage("{PropertyName} es requerido.")
            .NotNull()
            .MaximumLength(100).WithMessage("{PropertyName} no debe exceder los 100 caracteres.");

        RuleFor(p => p.Nombre)
            .NotEmpty().WithMessage("{PropertyName} es requerido.")
            .NotNull()
            .MaximumLength(250).WithMessage("{PropertyName} no debe exceder los 250 caracteres.");

        RuleFor(p => p.Apellido)
            .NotEmpty().WithMessage("{PropertyName} es requerido.")
            .NotNull()
            .MaximumLength(250).WithMessage("{PropertyName} no debe exceder los 250 caracteres.");

        RuleFor(p => p.Email)
            .NotEmpty().WithMessage("{PropertyName} es requerido.")
            .NotNull()
            .EmailAddress().WithMessage("{PropertyName} no es un email válido.")
            .MaximumLength(100).WithMessage("{PropertyName} no debe exceder los 100 caracteres.");

        RuleFor(p => p.Password)
            .NotEmpty().WithMessage("{PropertyName} es requerido.")
            .NotNull()
            .When(p => p.Id == 0);

        RuleFor(p => p.PerfilId)
             .NotEmpty().WithMessage("{PropertyName} es requerido.")
             .NotNull();
    }
}