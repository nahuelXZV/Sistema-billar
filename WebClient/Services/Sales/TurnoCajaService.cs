using Domain.DTOs.Sales;
using Domain.DTOs.Shared;
using Domain.Interfaces.Services.Sales;
using WebClient.Services.Implementacion;

namespace WebClient.Services.Sales;

public class TurnoCajaService : AppBaseServices, ITurnoCajaService
{
    public TurnoCajaService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor contextAccessor,
        ILogger<TurnoCajaService> logger)
        : base("api/TurnoCaja", httpClientFactory, contextAccessor, logger)
    {
    }

    public async Task<long> Create(TurnoCajaDTO turnoCaja)
    {
        return await PostAsync<long>(content: turnoCaja);
    }

    public async Task<bool> Delete(long id)
    {
        return await DeleteAsync<bool>($"Delete/{id}");
    }

    public async Task<bool> TieneActivo(long idVendedor)
    {
        return await GetAsync<bool>($"TieneActivo/{idVendedor}");
    }

    public async Task<ResponseFilterDTO<TurnoCajaDTO>> GetAll(FilterDTO? filter)
    {
        return await GetAsync<ResponseFilterDTO<TurnoCajaDTO>>(AplicarFiltro(filter));
    }

    public async Task<List<TurnoCajaDTO>> GetAll()
    {
        return await GetAsync<List<TurnoCajaDTO>>("GetAll");
    }

    public async Task<TurnoCajaDTO> GetById(long id)
    {
        return await GetAsync<TurnoCajaDTO>($"{id}");
    }

    public async Task<bool> Update(TurnoCajaDTO turnoCaja)
    {
        return await PutAsync<bool>(content: turnoCaja);
    }
}
