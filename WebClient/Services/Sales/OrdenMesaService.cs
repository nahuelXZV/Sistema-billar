using Domain.DTOs.Sales;
using Domain.Interfaces.Services.Sales;
using WebClient.Services.Implementacion;

namespace WebClient.Services.Sales;

public class OrdenMesaService : AppBaseServices, IOrdenMesaService
{
    public OrdenMesaService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor contextAccessor,
        ILogger<OrdenMesaService> logger)
        : base("api/OrdenMesa", httpClientFactory, contextAccessor, logger)
    {
    }

    public async Task<OrdenMesaDTO> Guardar(OrdenMesaDTO ordenMesa)
    {
        return await PostAsync<OrdenMesaDTO>("Guardar", ordenMesa);
    }

    public async Task<OrdenMesaDTO?> GetByMesa(long idMesa)
    {
        return await GetAsync<OrdenMesaDTO?>($"Mesa/{idMesa}");
    }

    public async Task<List<OrdenMesaDTO>> GetAbiertas()
    {
        return await GetAsync<List<OrdenMesaDTO>>("Abiertas");
    }

    public async Task<OrdenMesaDTO> IniciarCronometro(long idOrdenVenta)
    {
        return await PostAsync<OrdenMesaDTO>($"IniciarCronometro/{idOrdenVenta}", new { });
    }

    public async Task<OrdenMesaDTO> FinalizarCronometro(long idOrdenVenta)
    {
        return await PostAsync<OrdenMesaDTO>($"FinalizarCronometro/{idOrdenVenta}", new { });
    }
}
