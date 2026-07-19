using Domain.DTOs.Sales;
using Domain.DTOs.Shared;
using Domain.Interfaces.Services.Sales;
using WebClient.Services.Implementacion;

namespace WebClient.Services.Sales;

public class VentaService : AppBaseServices, IVentaService
{
    public VentaService(IHttpClientFactory httpClientFactory, IHttpContextAccessor contextAccessor, ILogger<VentaService> logger)
        : base("api/Venta", httpClientFactory, contextAccessor, logger)
    {
    }

    public async Task<long> Create(VentaDTO venta)
    {
        var uri = "";
        return await PostAsync<long>(uri, venta);
    }

    public async Task<bool> Delete(long id)
    {
        var uri = $"Delete/{id}";
        return await DeleteAsync<bool>(uri);
    }

    public async Task<ResponseFilterDTO<VentaDTO>> GetAll(FilterDTO? filter)
    {
        var uri = AplicarFiltro(filter);
        return await GetAsync<ResponseFilterDTO<VentaDTO>>(uri);
    }

    public async Task<List<VentaDTO>> GetAll()
    {
        var uri = "GetAll";
        return await GetAsync<List<VentaDTO>>(uri);
    }

    public async Task<VentaDTO> GetById(long id)
    {
        var uri = $"{id}";
        return await GetAsync<VentaDTO>(uri);
    }

    public async Task<decimal> GetMontoVendidoVendedor(long idVendedor, long idTurnoCaja)
    {
        var uri = $"GetMontoVendidoVendedor/{idVendedor}/{idTurnoCaja}";
        return await GetAsync<decimal>(uri);
    }

    public async Task<List<VentaMetodoPagoTotalDTO>> GetMontosVendidosPorMetodoPago(
        long idVendedor,
        long idTurnoCaja)
    {
        var uri = $"GetMontosVendidosPorMetodoPago/{idVendedor}/{idTurnoCaja}";
        return await GetAsync<List<VentaMetodoPagoTotalDTO>>(uri);
    }

    public async Task<bool> Update(VentaDTO venta)
    {
        var uri = "";
        return await PutAsync<bool>(uri, venta);
    }
}
