using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Security.Claims;
using WebClient.Extensions;

namespace WebClient.Common.Middlewares;

public class AppServicesAuthorizationHandler : DelegatingHandler
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AppServicesAuthorizationHandler(
        AuthenticationStateProvider authenticationStateProvider,
        IHttpContextAccessor httpContextAccessor)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var user = await GetCurrentUserAsync();

        if (user?.Identity?.IsAuthenticated == true)
        {
            var jwtAuthorizationToken = user.GetClaimValue(Constantes.ClaimTypes.Token);
            var idUsuario = user.GetClaimValue(Constantes.ClaimTypes.UsuarioId);

            if (!string.IsNullOrWhiteSpace(jwtAuthorizationToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtAuthorizationToken);

                if (!string.IsNullOrWhiteSpace(idUsuario))
                {
                    request.Headers.TryAddWithoutValidation(Constantes.ClaimTypes.UsuarioId, idUsuario);
                }
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<ClaimsPrincipal?> GetCurrentUserAsync()
    {
        try
        {
            var authenticationState =
                await _authenticationStateProvider.GetAuthenticationStateAsync();

            if (authenticationState.User.Identity?.IsAuthenticated == true)
            {
                return authenticationState.User;
            }
        }
        catch (InvalidOperationException)
        {
            // AuthenticationStateProvider is not initialized during regular MVC requests.
        }

        return _httpContextAccessor.HttpContext?.User;
    }
}
