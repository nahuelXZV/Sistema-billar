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
using Domain.Interfaces.Services.Inventory;
using WebClient.Services.Inventory;
using Domain.Validators.Inventory;

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
        services.AddValidatorsFromAssemblyContaining<CreateAlmacenDTOValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateCategoriaDTOValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateUnidadMedidaDTOValidator>();
        //services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        #endregion

        #region Services
        services.AddScoped<IAppServices, AppServices>();

        services.AddScoped<ISesionService, SesionService>();
        services.AddScoped<IPerfilService, PerfilService>();
        services.AddScoped<IModuloService, ModuloService>();
        services.AddScoped<IUsuarioService, UsuarioService>();

        services.AddScoped<IAlmacenService, AlmacenService>();
        services.AddScoped<ICategoriaService, CategoriaService>();
        services.AddScoped<IUnidadMedidaService, UnidadMedidaService>();
        #endregion

        return services;
    }
}
