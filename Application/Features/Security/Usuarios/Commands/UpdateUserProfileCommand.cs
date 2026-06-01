using Application.Features.Security.Usuarios.Queries;
using Application.Helpers;
using Application.Interfaces;
using Domain.Common;
using Domain.DTOs.Security;
using Domain.Entities.Security;
using Infraestructure.Interfaces;
using MediatR;

namespace Application.Features.Security.Usuarios.Commands;

public class UpdateUserProfileCommand : ICommand<Response<bool>>
{
    public required UsuarioPerfilDTO UsuarioPerfilDTO { get; set; }
}

public class UpdateUserProfileCommandHandler : ICommandHandler<UpdateUserProfileCommand, Response<bool>>
{
    private readonly IMediator _mediator;
    private readonly IRepository<Usuario> _repository;

    public UpdateUserProfileCommandHandler(IMediator mediator, IRepository<Usuario> repository)
    {
        _mediator = mediator;
        _repository = repository;
    }

    public async Task<Response<bool>> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var usuarioDto = request.UsuarioPerfilDTO;
        var usuario = await _repository.GetByIdAsync(usuarioDto.Id);

        if (usuario == null || usuario.Eliminado)
        {
            throw new ArgumentException("El usuario no existe.");
        }

        var existEmail = (await _mediator.Send(new GetUsuarioByEmailQuery { Email = usuarioDto.Email }, cancellationToken)).Data;
        if (existEmail != null && existEmail.Id != usuarioDto.Id)
        {
            throw new ArgumentException("El email ya se encuentra registrado por otro usuario.");
        }

        var existUsuario = (await _mediator.Send(new GetUserByUsernameQuery { Username = usuarioDto.Username }, cancellationToken)).Data;
        if (existUsuario != null && existUsuario.Id != usuarioDto.Id)
        {
            throw new ArgumentException("El usuario ya se encuentra registrado por otro usuario.");
        }

        var modificaPassword = usuarioDto.ModificarContrasena;
        var modificaUsername = !string.Equals(usuario.Username, usuarioDto.Username, StringComparison.Ordinal);

        if (modificaPassword || modificaUsername)
        {
            if (string.IsNullOrWhiteSpace(usuarioDto.PasswordActual))
            {
                throw new ArgumentException("La contrasena actual es requerida.");
            }

            var passwordValido = PasswordHasherHelper.VerifyPassword(usuario.Username, usuario.Password, usuarioDto.PasswordActual);
            if (!passwordValido)
            {
                throw new ArgumentException("La contrasena actual no es correcta.");
            }
        }

        if (modificaPassword)
        {
            usuario.Password = PasswordHasherHelper.HashPassword(usuarioDto.Username, usuarioDto.NuevaPassword);
        }
        else if (modificaUsername)
        {
            usuario.Password = PasswordHasherHelper.HashPassword(usuarioDto.Username, usuarioDto.PasswordActual);
        }

        usuario.Username = usuarioDto.Username;
        usuario.Nombre = usuarioDto.Nombre;
        usuario.Apellido = usuarioDto.Apellido;
        usuario.Email = usuarioDto.Email;

        _repository.Update(usuario);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new Response<bool>(true);
    }
}
