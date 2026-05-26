using Domain.DTOs.Sales;
using Domain.DTOs.Shared;
using Domain.Interfaces.Services.Sales;
using WebClient.Services.Implementacion;

namespace WebClient.Services.Sales;

public class UsoMesaService : AppBaseServices, IUsoMesaService
{
    public UsoMesaService(IHttpClientFactory httpClientFactory, IHttpContextAccessor contextAccessor, ILogger<UsoMesaService> logger)
        : base("api/UsoMesa", httpClientFactory, contextAccessor, logger)
    {
    }

    public async Task<long> Create(UsoMesaDTO usoMesa)
    {
        var uri = "";
        return await PostAsync<long>(uri, usoMesa);
    }

    public async Task<bool> Delete(long id)
    {
        var uri = $"Delete/{id}";
        return await DeleteAsync<bool>(uri);
    }

    public async Task<ResponseFilterDTO<UsoMesaDTO>> GetAll(FilterDTO? filter)
    {
        var uri = AplicarFiltro(filter);
        return await GetAsync<ResponseFilterDTO<UsoMesaDTO>>(uri);
    }

    public async Task<List<UsoMesaDTO>> GetAll()
    {
        var uri = "GetAll";
        return await GetAsync<List<UsoMesaDTO>>(uri);
    }

    public async Task<UsoMesaDTO> GetById(long id)
    {
        var uri = $"{id}";
        return await GetAsync<UsoMesaDTO>(uri);
    }

    public async Task<bool> Update(UsoMesaDTO usoMesa)
    {
        var uri = "";
        return await PutAsync<bool>(uri, usoMesa);
    }
}
