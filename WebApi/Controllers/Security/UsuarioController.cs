using Application.Features.Security.Usuarios.Commands;
using Application.Features.Security.Usuarios.Queries;
using Domain.DTOs.Security;
using Domain.DTOs.Shared;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Security;

public class UsuarioController : MainController
{
    private readonly ILogger<UsuarioController> _logger;

    public UsuarioController(ILogger<UsuarioController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] FilterDTO? filter)
    {
        return Ok(await Mediator.Send(new GetAllUsersQuery() { Filter = filter }));
    }

    [HttpGet("{idUser}")]
    public async Task<IActionResult> GetById(long idUser)
    {
        return Ok(await Mediator.Send(new GetUserByIdQuery() { Id = idUser }));
    }

    [HttpPost]
    public async Task<IActionResult> Create(UsuarioDTO usuarioDTO)
    {
        return Ok(await Mediator.Send(new CreateUserCommand { UsuarioDTO = usuarioDTO }));
    }

    [HttpPut]
    public async Task<IActionResult> Update(UsuarioDTO usuarioDTO)
    {
        return Ok(await Mediator.Send(new UpdateUserCommand { UsuarioDTO = usuarioDTO }));
    }

    [HttpPut("Perfil")]
    public async Task<IActionResult> UpdatePerfil(UsuarioPerfilDTO usuarioPerfilDTO)
    {
        usuarioPerfilDTO.Id = GetLoggedUserId();
        return Ok(await Mediator.Send(new UpdateUserProfileCommand { UsuarioPerfilDTO = usuarioPerfilDTO }));
    }

    [HttpDelete("Delete/{idUser}")]
    public async Task<IActionResult> Delete(long idUser)
    {
        return Ok(await Mediator.Send(new DeleteUserCommand { Id = idUser }));
    }

    private long GetLoggedUserId()
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue("sub")
            ?? Request.Headers["UsuarioId"].FirstOrDefault();

        if (!long.TryParse(userId, out var idUser) || idUser == 0)
        {
            throw new UnauthorizedAccessException("No se pudo identificar al usuario logeado.");
        }

        return idUser;
    }
}
