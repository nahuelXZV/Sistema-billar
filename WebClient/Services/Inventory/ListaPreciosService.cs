using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;
using Domain.Interfaces.Services.Inventory;
using WebClient.Services.Implementacion;

namespace WebClient.Services.Inventory;

public class ListaPreciosService : AppBaseServices, IListaPreciosService
{
    public ListaPreciosService(IHttpClientFactory httpClientFactory, IHttpContextAccessor contextAccessor, ILogger<ListaPreciosService> logger)
        : base("api/ListaPrecios", httpClientFactory, contextAccessor, logger)
    {
    }

    public async Task<long> Create(ListaPrecioDTO listaPrecio)
    {
        var uri = $"";
        return await PostAsync<long>(uri, listaPrecio);
    }

    public async Task<bool> Delete(long id)
    {
        var uri = $"Delete/{id}";
        return await DeleteAsync<bool>(uri);
    }

    public async Task<ResponseFilterDTO<ListaPrecioDTO>> GetAll(FilterDTO? filter)
    {
        var uri = AplicarFiltro(filter);
        return await GetAsync<ResponseFilterDTO<ListaPrecioDTO>>(uri);
    }

    public async Task<List<ListaPrecioDTO>> GetAll()
    {
        var uri = "GetAll";
        return await GetAsync<List<ListaPrecioDTO>>(uri);
    }

    public async Task<ListaPrecioDTO> GetById(long id)
    {
        var uri = $"{id}";
        return await GetAsync<ListaPrecioDTO>(uri);
    }

    public async Task<bool> Update(ListaPrecioDTO producto)
    {
        var uri = $"";
        return await PutAsync<bool>(uri, producto);
    }
}
