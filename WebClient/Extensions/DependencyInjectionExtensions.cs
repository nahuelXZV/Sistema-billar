using System.Reflection;
using Domain.Constants;
using FluentValidation;
using WebClient.Configs;
using WebClient.Common.Middlewares;
using WebClient.Services.Implementacion;
using WebClient.Services;
using Domain.Interfaces.Services.Security;
using WebClient.Services.Segurity;
using Domain.Validators.Security;

namespace WebClient.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, AdminConfig configs)
    {
        services.AddTransient<AppServicesAuthorizationHandler>();
        services.AddHttpClient(Constantes.HttpClientNames.ApiRest, client =>
        {
            client.BaseAddress = new Uri(configs.General.ApiUrl);
            client.Timeout = TimeSpan.FromSeconds(configs.General.ServiceTimeout);
        }).AddHttpMessageHandler<AppServicesAuthorizationHandler>();
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        #region Validators
        services.AddValidatorsFromAssemblyContaining<CreateUsuarioDTOValidator>();
        #endregion

        #region Services
        services.AddScoped<IAppServices, AppServices>();

        services.AddScoped<ISesionService, SesionService>();
        services.AddScoped<IPerfilService, PerfilService>();
        services.AddScoped<IModuloService, ModuloService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        #endregion

        return services;
    }
}
