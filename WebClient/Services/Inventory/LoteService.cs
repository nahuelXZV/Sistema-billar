using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;
using Domain.Interfaces.Services.Inventory;
using WebClient.Services.Implementacion;

namespace WebClient.Services.Inventory;

public class LoteService : AppBaseServices, ILoteService
{
    public LoteService(IHttpClientFactory httpClientFactory, IHttpContextAccessor contextAccessor, ILogger<LoteService> logger)
        : base("api/Lote", httpClientFactory, contextAccessor, logger)
    {
    }

    public async Task<long> Create(LoteDTO lote)
    {
        var uri = $"";
        return await PostAsync<long>(uri, lote);
    }

    public async Task<bool> Delete(long id)
    {
        var uri = $"Delete/{id}";
        return await DeleteAsync<bool>(uri);
    }

    public async Task<ResponseFilterDTO<LoteDTO>> GetAll(FilterDTO? filter)
    {
        var uri = AplicarFiltro(filter);
        return await GetAsync<ResponseFilterDTO<LoteDTO>>(uri);
    }

    public async Task<List<LoteDTO>> GetAll()
    {
        var uri = "GetAll";
        return await GetAsync<List<LoteDTO>>(uri);
    }

    public async Task<LoteDTO> GetById(long id)
    {
        var uri = $"{id}";
        return await GetAsync<LoteDTO>(uri);
    }

    public async Task<bool> Update(LoteDTO lote)
    {
        var uri = $"";
        return await PutAsync<bool>(uri, lote);
    }
}
