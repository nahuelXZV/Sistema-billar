using Domain.DTOs.Inventory;
using Domain.Interfaces.Services.Inventory;
using WebClient.Services.Implementacion;

namespace WebClient.Services.Inventory;

public class CategoriaService : AppBaseServices, ICategoriaService
{
    public CategoriaService(IHttpClientFactory httpClientFactory, IHttpContextAccessor contextAccessor, ILogger<CategoriaService> logger)
        : base("api/Categoria", httpClientFactory, contextAccessor, logger)
    {
    }

    public async Task<long> Create(CategoriaDTO categoria)
    {
        var uri = $"";
        return await PostAsync<long>(uri, categoria);
    }

    public async Task<bool> Delete(long id)
    {
        var uri = $"Delete/{id}";
        return await DeleteAsync<bool>(uri);
    }

    public async Task<List<CategoriaDTO>> GetAll()
    {
        var uri = "";
        return await GetAsync<List<CategoriaDTO>>(uri);
    }

    public async Task<List<CategoriaDTO>> GetAllSinNivel()
    {
        var uri = "SinNivel";
        return await GetAsync<List<CategoriaDTO>>(uri);
    }

    public async Task<List<CategoriaDTO>> GetCategoriasBase()
    {
        var uri = "Base";
        return await GetAsync<List<CategoriaDTO>>(uri);
    }

    public async Task<List<CategoriaDTO>> GetByCategoriaPadre(long idCategoriaPadre)
    {
        var uri = $"PorPadre/{idCategoriaPadre}";
        return await GetAsync<List<CategoriaDTO>>(uri);
    }

    public async Task<CategoriaDTO> GetById(long id)
    {
        var uri = $"{id}";
        return await GetAsync<CategoriaDTO>(uri);
    }

    public async Task<bool> Update(CategoriaDTO categoria)
    {
        var uri = $"";
        return await PutAsync<bool>(uri, categoria);
    }
}
