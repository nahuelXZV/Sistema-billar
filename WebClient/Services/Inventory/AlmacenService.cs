using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;
using Domain.Interfaces.Services.Inventory;
using WebClient.Services.Implementacion;

namespace WebClient.Services.Inventory;

public class AlmacenService : AppBaseServices, IAlmacenService
{
    public AlmacenService(IHttpClientFactory httpClientFactory, IHttpContextAccessor contextAccessor, ILogger<AlmacenService> logger)
        : base("api/Almacen", httpClientFactory, contextAccessor, logger)
    {
    }

    public async Task<long> Create(AlmacenDTO almacen)
    {
        var uri = $"";
        return await PostAsync<long>(uri, almacen);
    }

    public async Task<bool> Delete(long id)
    {
        var uri = $"Delete/{id}";
        return await DeleteAsync<bool>(uri);
    }

    public async Task<ResponseFilterDTO<AlmacenDTO>> GetAll(FilterDTO? filter)
    {
        var uri = AplicarFiltro(filter);
        return await GetAsync<ResponseFilterDTO<AlmacenDTO>>(uri);
    }

    public async Task<AlmacenDTO> GetById(long id)
    {
        var uri = $"{id}";
        return await GetAsync<AlmacenDTO>(uri);
    }

    public async Task<bool> Update(AlmacenDTO almacen)
    {
        var uri = $"";
        return await PutAsync<bool>(uri, almacen);
    }
}
