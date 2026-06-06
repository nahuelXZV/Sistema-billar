using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;
using Domain.Interfaces.Services.Inventory;
using WebClient.Services.Implementacion;

namespace WebClient.Services.Inventory;

public class ProductoService : AppBaseServices, IProductoService
{
    public ProductoService(IHttpClientFactory httpClientFactory, IHttpContextAccessor contextAccessor, ILogger<ProductoService> logger)
        : base("api/Producto", httpClientFactory, contextAccessor, logger)
    {
    }

    public async Task<long> Create(ProductoDTO producto)
    {
        var uri = $"";
        return await PostAsync<long>(uri, producto);
    }

    public async Task<bool> Delete(long id)
    {
        var uri = $"Delete/{id}";
        return await DeleteAsync<bool>(uri);
    }

    public async Task<ResponseFilterDTO<ProductoDTO>> GetAll(FilterDTO? filter)
    {
        var uri = AplicarFiltro(filter);
        return await GetAsync<ResponseFilterDTO<ProductoDTO>>(uri);
    }

    public async Task<List<ProductoDTO>> GetAll()
    {
        var uri = "GetAll";
        return await GetAsync<List<ProductoDTO>>(uri);
    }

    public async Task<List<ProductoDTO>> GetByCategoria(long idCategoria, long idVendedor)
    {
        var uri = $"PorCategoria/{idCategoria}/Vendedor/{idVendedor}";
        return await GetAsync<List<ProductoDTO>>(uri);
    }

    public async Task<ProductoDTO> GetById(long id)
    {
        var uri = $"{id}";
        return await GetAsync<ProductoDTO>(uri);
    }

    public async Task<ProductoDTO> GetById(long id, long idVendedor)
    {
        var uri = $"{id}/Vendedor/{idVendedor}";
        return await GetAsync<ProductoDTO>(uri);
    }

    public async Task<bool> Update(ProductoDTO producto)
    {
        var uri = $"";
        return await PutAsync<bool>(uri, producto);
    }
}
