using Microsoft.Extensions.DependencyInjection;
using Infraestructure.Repositories;
using Infraestructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Infraestructure.Interfaces;

namespace Infraestructure;

public static class DependencyInjection
{
    public static void AddInfrastructureBase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>((options) =>
        {
            options.UseSqlServer(configuration.GetConnectionString("ApplicationDbContext"));
        });
        services.AddScoped(typeof(IRepository<>), typeof(EntityFrameworkRepository<>));
        services.AddScoped<IDbContext>(provider =>
            provider.GetRequiredService<AppDbContext>());
    }
}
