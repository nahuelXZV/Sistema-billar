using Domain.Constants;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public abstract class MainController : ControllerBase
{
    private IMediator? _mediator;
    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

    protected long IdUsuarioActual
    {
        get
        {
            var idUsuarioClaim = User.FindFirst(JwtClaimNames.IdUsuario)?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            if (!long.TryParse(idUsuarioClaim, out var idUsuario) || idUsuario <= 0)
            {
                throw new UnauthorizedAccessException("No se pudo identificar al usuario autenticado.");
            }

            return idUsuario;
        }
    }

    protected string? NombreUsuarioActual => User.FindFirst(JwtClaimNames.Nombre)?.Value;

    protected string? EmailUsuarioActual => User.FindFirst(JwtClaimNames.Email)?.Value;

    protected string? UsernameActual => User.FindFirst(JwtClaimNames.Usuario)?.Value;
}
