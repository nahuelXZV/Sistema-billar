using Domain.DTOs.Sales;
using Domain.DTOs.Shared;
using Domain.Interfaces.Services.Sales;
using WebClient.Services.Implementacion;

namespace WebClient.Services.Sales;

public class VendedorService : AppBaseServices, IVendedorService
{
    public VendedorService(IHttpClientFactory httpClientFactory, IHttpContextAccessor contextAccessor, ILogger<VendedorService> logger)
        : base("api/Vendedor", httpClientFactory, contextAccessor, logger)
    {
    }

    public async Task<long> Create(VendedorDTO vendedor)
    {
        var uri = $@"";
        return await PostAsync<long>(uri, vendedor);
    }

    public async Task<bool> Delete(long id)
    {
        var uri = $"Delete/{id}";
        return await DeleteAsync<bool>(uri);
    }

    public async Task<ResponseFilterDTO<VendedorDTO>> GetAll(FilterDTO? filter)
    {
        var uri = AplicarFiltro(filter);
        return await GetAsync<ResponseFilterDTO<VendedorDTO>>(uri);
    }

    public async Task<List<VendedorDTO>> GetAll()
    {
        var uri = "GetAll";
        return await GetAsync<List<VendedorDTO>>(uri);
    }

    public async Task<VendedorDTO> GetById(long id)
    {
        var uri = $"{id}";
        return await GetAsync<VendedorDTO>(uri);
    }

    public async Task<VendedorDTO> GetByUsuario(long idUsuario)
    {
        var uri = $"PorUsuario/{idUsuario}";
        return await GetAsync<VendedorDTO>(uri);
    }

    public async Task<bool> Update(VendedorDTO vendedor)
    {
        var uri = $@"";
        return await PutAsync<bool>(uri, vendedor);
    }
}
