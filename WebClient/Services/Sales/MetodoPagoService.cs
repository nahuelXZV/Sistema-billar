using Domain.DTOs.Sales;
using Domain.DTOs.Shared;
using Domain.Interfaces.Services.Sales;
using WebClient.Services.Implementacion;

namespace WebClient.Services.Sales;

public class MetodoPagoService : AppBaseServices, IMetodoPagoService
{
    public MetodoPagoService(IHttpClientFactory httpClientFactory, IHttpContextAccessor contextAccessor, ILogger<MetodoPagoService> logger)
        : base("api/MetodoPago", httpClientFactory, contextAccessor, logger)
    {
    }

    public async Task<long> Create(MetodoPagoDTO metodoPago)
    {
        var uri = "";
        return await PostAsync<long>(uri, metodoPago);
    }

    public async Task<bool> Delete(long id)
    {
        var uri = $"Delete/{id}";
        return await DeleteAsync<bool>(uri);
    }

    public async Task<ResponseFilterDTO<MetodoPagoDTO>> GetAll(FilterDTO? filter)
    {
        var uri = AplicarFiltro(filter);
        return await GetAsync<ResponseFilterDTO<MetodoPagoDTO>>(uri);
    }

    public async Task<List<MetodoPagoDTO>> GetAll()
    {
        var uri = "GetAll";
        return await GetAsync<List<MetodoPagoDTO>>(uri);
    }

    public async Task<MetodoPagoDTO> GetById(long id)
    {
        var uri = $"{id}";
        return await GetAsync<MetodoPagoDTO>(uri);
    }

    public async Task<bool> Update(MetodoPagoDTO metodoPago)
    {
        var uri = "";
        return await PutAsync<bool>(uri, metodoPago);
    }
}
