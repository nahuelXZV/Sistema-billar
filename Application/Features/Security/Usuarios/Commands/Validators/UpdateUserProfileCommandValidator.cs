using Domain.Validators.Security;
using FluentValidation;

namespace Application.Features.Security.Usuarios.Commands.Validators;

public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(command => command.UsuarioPerfilDTO)
            .SetValidator(new UsuarioPerfilDTOValidator());
    }
}
