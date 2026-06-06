using Domain.DTOs.General;
using Domain.Interfaces.Services.General;
using WebClient.Services.Implementacion;

namespace WebClient.Services.General;

public class DashboardService : AppBaseServices, IDashboardService
{
    public DashboardService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor contextAccessor,
        ILogger<DashboardService> logger)
        : base("api/Dashboard", httpClientFactory, contextAccessor, logger)
    {
    }

    public async Task<DashboardDTO> Get()
    {
        return await GetAsync<DashboardDTO>();
    }
}
