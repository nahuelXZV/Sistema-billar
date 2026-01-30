using Microsoft.EntityFrameworkCore;
using MediatR;
using Domain.Interfaces.Shared;
using Domain.Common;
using Domain.Entities.Security;
using Domain.DTOs.Security;

namespace Application.Features.Security.Usuarios.Queries;

public class GetUsuarioByEmailQuery : IRequest<Response<UsuarioDTO>>
{
    public string Email { get; set; }
}

public class GetUsuarioByEmailQueryHandler : IRequestHandler<GetUsuarioByEmailQuery, Response<UsuarioDTO>>
{
    private readonly IDbContext _appCnx;
    private readonly IMediator _mediator;

    public GetUsuarioByEmailQueryHandler(IDbContext appDbContext, IMediator mediator)
    {
        _appCnx = appDbContext;
        _mediator = mediator;
    }
    public async Task<Response<UsuarioDTO>> Handle(GetUsuarioByEmailQuery request, CancellationToken cancellationToken)
    {
        var dbContext = _appCnx.dbContext;
        var usuarioRequest = await dbContext.Set<Usuario>()
                                        .Where(u => u.Email == request.Email)
                                        .Select(u => new UsuarioDTO
                                        {
                                            Id = u.Id,
                                            Username = u.Username,
                                            Nombre = u.Nombre,
                                            Apellido = u.Apellido,
                                            Password = u.Password,
                                            Email = u.Email,
                                        })
                                        .SingleOrDefaultAsync(cancellationToken);

        if (usuarioRequest == null) return new Response<UsuarioDTO>("Usuario no encontrado.");

        return new Response<UsuarioDTO>(usuarioRequest);
    }
}