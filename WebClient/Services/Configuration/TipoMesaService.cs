using Domain.DTOs.Configuration;
using Domain.DTOs.Shared;
using Domain.Interfaces.Services.Configuration;
using WebClient.Services.Implementacion;

namespace WebClient.Services.Configuration;

public class TipoMesaService : AppBaseServices, ITipoMesaService
{
    public TipoMesaService(IHttpClientFactory httpClientFactory, IHttpContextAccessor contextAccessor, ILogger<TipoMesaService> logger)
        : base("api/TipoMesa", httpClientFactory, contextAccessor, logger)
    {
    }

    public async Task<long> Create(TipoMesaDTO tipo)
    {
        var uri = $"";
        return await PostAsync<long>(uri, tipo);
    }

    public async Task<bool> Delete(long id)
    {
        var uri = $"Delete/{id}";
        return await DeleteAsync<bool>(uri);
    }

    public async Task<ResponseFilterDTO<TipoMesaDTO>> GetAll(FilterDTO? filter)
    {
        var uri = AplicarFiltro(filter);
        return await GetAsync<ResponseFilterDTO<TipoMesaDTO>>(uri);
    }

    public async Task<List<TipoMesaDTO>> GetAll()
    {
        var uri = "GetAll";
        return await GetAsync<List<TipoMesaDTO>>(uri);
    }

    public async Task<TipoMesaDTO> GetById(long id)
    {
        var uri = $"{id}";
        return await GetAsync<TipoMesaDTO>(uri);
    }

    public async Task<bool> Update(TipoMesaDTO tipo)
    {
        var uri = $"";
        return await PutAsync<bool>(uri, tipo);
    }
}
