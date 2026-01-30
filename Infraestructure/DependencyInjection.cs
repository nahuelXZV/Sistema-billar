using Microsoft.Extensions.DependencyInjection;
using Infraestructure.Repositories;
using Infraestructure.Persistence;
using Domain.Interfaces.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

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
        services.AddScoped<IDbContext, AppDbContext>();

    }
}
