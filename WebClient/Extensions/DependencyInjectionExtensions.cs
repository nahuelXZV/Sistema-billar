using System.Reflection;
using Domain.Constants;
using Domain.Interfaces.Services.Contact;
using Domain.Interfaces.Services.General;
using FluentValidation;
using WebClient.Configs;
using WebClient.Common.Middlewares;
using WebClient.Services.Implementacion;
using Domain.Interfaces.Services.Security;
using Domain.Validators.Security;
using Domain.Interfaces.Services.Inventory;
using Domain.Interfaces.Services.Sales;
using Domain.Interfaces.Services.Shared;
using WebClient.Services.Inventory;
using Domain.Validators.Inventory;
using Domain.Interfaces.Services.Configuration;
using WebClient.Services.Configuration;
using Domain.Validators.Contact;
using Domain.Validators.Configuration;
using Domain.Validators.Sales;
using WebClient.Services.Contact;
using WebClient.Services.General;
using WebClient.Services.Sales;
using WebClient.Services.Security;
using WebClient.Services.Shared;
using WebClient.Services;

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
        services.AddValidatorsFromAssemblyContaining<CreateProductoDTOValidartor>();
        services.AddValidatorsFromAssemblyContaining<CreateListaPreciosDTOValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateTransaccionInventarioDTOValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateLoteDTOValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateTipoMesaDTOValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateVendedorDTOValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateMetodoPagoDTOValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateClienteDTOValidator>();
        //services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());CreateTransaccionInventarioDTOValidator
        #endregion

        #region Services
        services.AddScoped<IAppServices, AppServices>();

        services.AddScoped<ISesionService, SesionService>();
        services.AddScoped<IPerfilService, PerfilService>();
        services.AddScoped<IModuloService, ModuloService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IDashboardService, DashboardService>();

        services.AddScoped<IAlmacenService, AlmacenService>();
        services.AddScoped<ICategoriaService, CategoriaService>();
        services.AddScoped<IUnidadMedidaService, UnidadMedidaService>();
        services.AddScoped<IProductoService, ProductoService>();
        services.AddScoped<IInventarioService, InventarioService>();
        services.AddScoped<ITransaccionInventarioService, TransaccionInventarioService>();
        services.AddScoped<IListaPreciosService, ListaPreciosService>();
        services.AddScoped<ILoteService, LoteService>();
        services.AddScoped<ITipoMesaService, TipoMesaService>();
        services.AddScoped<IMesasService, MesasService>();
        services.AddScoped<IVendedorService, VendedorService>();
        services.AddScoped<IMetodoPagoService, MetodoPagoService>();
        services.AddScoped<IUsoMesaService, UsoMesaService>();
        services.AddScoped<IOrdenVentaService, OrdenVentaService>();
        services.AddScoped<IOrdenMesaService, OrdenMesaService>();
        services.AddScoped<IVentaService, VentaService>();
        services.AddScoped<IArchivoService, ArchivoService>();
        #endregion

        return services;
    }
}
