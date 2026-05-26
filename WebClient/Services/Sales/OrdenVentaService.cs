using Domain.DTOs.Sales;
using Domain.DTOs.Shared;
using Domain.Interfaces.Services.Sales;
using WebClient.Services.Implementacion;

namespace WebClient.Services.Sales;

public class OrdenVentaService : AppBaseServices, IOrdenVentaService
{
    public OrdenVentaService(IHttpClientFactory httpClientFactory, IHttpContextAccessor contextAccessor, ILogger<OrdenVentaService> logger)
        : base("api/OrdenVenta", httpClientFactory, contextAccessor, logger)
    {
    }

    public async Task<long> Create(OrdenVentaDTO ordenVenta)
    {
        var uri = "";
        return await PostAsync<long>(uri, ordenVenta);
    }

    public async Task<bool> Delete(long id)
    {
        var uri = $"Delete/{id}";
        return await DeleteAsync<bool>(uri);
    }

    public async Task<ResponseFilterDTO<OrdenVentaDTO>> GetAll(FilterDTO? filter)
    {
        var uri = AplicarFiltro(filter);
        return await GetAsync<ResponseFilterDTO<OrdenVentaDTO>>(uri);
    }

    public async Task<List<OrdenVentaDTO>> GetAll()
    {
        var uri = "GetAll";
        return await GetAsync<List<OrdenVentaDTO>>(uri);
    }

    public async Task<OrdenVentaDTO> GetById(long id)
    {
        var uri = $"{id}";
        return await GetAsync<OrdenVentaDTO>(uri);
    }

    public async Task<bool> Update(OrdenVentaDTO ordenVenta)
    {
        var uri = "";
        return await PutAsync<bool>(uri, ordenVenta);
    }
}
