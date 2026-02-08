using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;
using Domain.Interfaces.Services.Inventory;
using WebClient.Services.Implementacion;

namespace WebClient.Services.Inventory;

public class UnidadMedidaService : AppBaseServices, IUnidadMedidaService
{
    public UnidadMedidaService(IHttpClientFactory httpClientFactory, IHttpContextAccessor contextAccessor, ILogger<UnidadMedidaService> logger)
        : base("api/UnidadMedida", httpClientFactory, contextAccessor, logger)
    {
    }

    public async Task<long> Create(UnidadMedidaDTO unidad)
    {
        var uri = $"";
        return await PostAsync<long>(uri, unidad);
    }

    public async Task<bool> Delete(long id)
    {
        var uri = $"Delete/{id}";
        return await DeleteAsync<bool>(uri);
    }

    public async Task<ResponseFilterDTO<UnidadMedidaDTO>> GetAll(FilterDTO? filter)
    {
        var uri = AplicarFiltro(filter);
        return await GetAsync<ResponseFilterDTO<UnidadMedidaDTO>>(uri);
    }
    public async Task<List<UnidadMedidaDTO>> GetAll()
    {
        var uri = "GetAll";
        return await GetAsync<List<UnidadMedidaDTO>>(uri);
    }

    public async Task<UnidadMedidaDTO> GetById(long id)
    {
        var uri = $"{id}";
        return await GetAsync<UnidadMedidaDTO>(uri);
    }

    public async Task<bool> Update(UnidadMedidaDTO unidad)
    {
        var uri = $"";
        return await PutAsync<bool>(uri, unidad);
    }
}
