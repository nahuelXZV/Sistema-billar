using Domain.Validators.Security;
using FluentValidation;

namespace Application.Features.Security.Usuarios.Commands.Validators;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(command => command.UsuarioDTO)
            .SetValidator(new CreateUsuarioDTOValidator());

    }
}

