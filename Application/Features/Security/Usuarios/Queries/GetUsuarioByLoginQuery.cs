using Microsoft.EntityFrameworkCore;
using MediatR;
using Domain.Interfaces.Shared;
using Domain.Common;
using Domain.Entities.Security;
using Domain.DTOs.Security;

namespace Application.Features.Security.Usuarios.Queries;

public class GetUsuarioByLoginQuery : IRequest<Response<UsuarioDTO>>
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class GetUsuarioByLoginQueryHandler : IRequestHandler<GetUsuarioByLoginQuery, Response<UsuarioDTO>>
{
    private readonly IDbContext _appCnx;
    private readonly IMediator _mediator;

    public GetUsuarioByLoginQueryHandler(IDbContext appDbContext, IMediator mediator)
    {
        _appCnx = appDbContext;
        _mediator = mediator;
    }
    public async Task<Response<UsuarioDTO>> Handle(GetUsuarioByLoginQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var dbContext = _appCnx.dbContext;
            string email = request.Email;
            string password = request.Password;

            var usuarioRequest = await dbContext.Set<Usuario>()
                                        .Where(u => u.Email == email && u.Password == password)
                                        .Select(u => new UsuarioDTO
                                        {
                                            Id = u.Id,
                                            Username = u.Username,
                                            Nombre = u.Nombre,
                                            Apellido = u.Apellido,
                                            Email = u.Email,
                                        })
                                        .SingleOrDefaultAsync(cancellationToken);

            if (usuarioRequest == null) return new Response<UsuarioDTO>("Usuario no encontrado.");
            return new Response<UsuarioDTO>(usuarioRequest);
        }
        catch (Exception ex)
        {
            return new Response<UsuarioDTO>($"Error al obtener el usuario: {ex.Message}");
        }
    }
}