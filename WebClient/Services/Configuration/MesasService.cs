using Domain.DTOs.Configuration;
using Domain.DTOs.Shared;
using Domain.Interfaces.Services.Configuration;
using WebClient.Services.Implementacion;

namespace WebClient.Services.Configuration;

public class MesasService : AppBaseServices, IMesasService
{
    public MesasService(IHttpClientFactory httpClientFactory, IHttpContextAccessor contextAccessor, ILogger<MesasService> logger)
        : base("api/Mesas", httpClientFactory, contextAccessor, logger)
    {
    }

    public async Task<long> Create(MesaDTO mesa)
    {
        var uri = $"";
        return await PostAsync<long>(uri, mesa);
    }

    public async Task<bool> Delete(long id)
    {
        var uri = $"Delete/{id}";
        return await DeleteAsync<bool>(uri);
    }

    public async Task<ResponseFilterDTO<MesaDTO>> GetAll(FilterDTO? filter)
    {
        var uri = AplicarFiltro(filter);
        return await GetAsync<ResponseFilterDTO<MesaDTO>>(uri);
    }

    public async Task<List<MesaDTO>> GetAll()
    {
        var uri = "GetAll";
        return await GetAsync<List<MesaDTO>>(uri);
    }

    public async Task<MesaDTO> GetById(long id)
    {
        var uri = $"{id}";
        return await GetAsync<MesaDTO>(uri);
    }

    public async Task<bool> Update(MesaDTO mesa)
    {
        var uri = $"";
        return await PutAsync<bool>(uri, mesa);
    }
}
