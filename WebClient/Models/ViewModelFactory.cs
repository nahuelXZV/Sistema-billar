namespace WebClient.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using WebClient.Configs;

public class ViewModelFactory
{
    private readonly AdminConfig _adminConfig;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ViewModelFactory(IOptions<AdminConfig> adminConfigOptions, IHttpContextAccessor httpContextAccessor)
    {
        _adminConfig = adminConfigOptions.Value;
        _httpContextAccessor = httpContextAccessor;
    }

    public T Create<T>() where T : IMainViewModel, new()
    {
        var instance = new T();
        var context = _httpContextAccessor.HttpContext;

        if (context == null)
            throw new InvalidOperationException("HttpContext no esta disponible.");

        instance.Initialize(context, _adminConfig);

        var permiteAnonimo = context.GetEndpoint()?.Metadata?.GetMetadata<IAllowAnonymous>() != null;
        if (!permiteAnonimo && instance is MainViewModel mainViewModel && !mainViewModel.SesionUsuarioValida)
        {
            context.Session.Clear();
            throw new UnauthorizedAccessException("La sesion ha expirado o no es valida.");
        }

        return instance;
    }
}
