using Domain.DTOs.Security;
using FluentValidation;

namespace Domain.Validators.Security;

public class UsuarioPerfilDTOValidator : AbstractValidator<UsuarioPerfilDTO>
{
    public UsuarioPerfilDTOValidator()
    {
        RuleFor(p => p.Username)
            .NotEmpty().WithMessage("{PropertyName} es requerido.")
            .MaximumLength(100).WithMessage("{PropertyName} no debe exceder los 100 caracteres.");

        RuleFor(p => p.Nombre)
            .NotEmpty().WithMessage("{PropertyName} es requerido.")
            .MaximumLength(250).WithMessage("{PropertyName} no debe exceder los 250 caracteres.");

        RuleFor(p => p.Apellido)
            .NotEmpty().WithMessage("{PropertyName} es requerido.")
            .MaximumLength(250).WithMessage("{PropertyName} no debe exceder los 250 caracteres.");

        RuleFor(p => p.Email)
            .NotEmpty().WithMessage("{PropertyName} es requerido.")
            .EmailAddress().WithMessage("{PropertyName} no es un email valido.")
            .MaximumLength(100).WithMessage("{PropertyName} no debe exceder los 100 caracteres.");

        RuleFor(p => p.PasswordActual)
            .NotEmpty().WithMessage("La contrasena actual es requerida para modificar la contrasena.")
            .When(p => p.ModificarContrasena);

        RuleFor(p => p.NuevaPassword)
            .NotEmpty().WithMessage("La nueva contrasena es requerida.")
            .MinimumLength(6).WithMessage("La nueva contrasena debe tener al menos 6 caracteres.")
            .When(p => p.ModificarContrasena);

        RuleFor(p => p.ConfirmarPassword)
            .Equal(p => p.NuevaPassword).WithMessage("La confirmacion de contrasena no coincide.")
            .When(p => p.ModificarContrasena);
    }
}
