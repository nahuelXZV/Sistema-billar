using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;
using Domain.Interfaces.Services.Inventory;
using WebClient.Services.Implementacion;

namespace WebClient.Services.Inventory;

public class TransaccionInventarioService : AppBaseServices, ITransaccionInventarioService
{
    public TransaccionInventarioService(IHttpClientFactory httpClientFactory, IHttpContextAccessor contextAccessor, ILogger<TransaccionInventarioService> logger)
        : base("api/TransaccionInventario", httpClientFactory, contextAccessor, logger)
    {
    }

    public async Task<long> Create(TransaccionInventarioDTO transaccion)
    {
        var uri = $"";
        return await PostAsync<long>(uri, transaccion);
    }

    public async Task<ResponseFilterDTO<TransaccionInventarioDetalleDTO>> GetAll(FilterDTO? filter)
    {
        var uri = AplicarFiltro(filter);
        return await GetAsync<ResponseFilterDTO<TransaccionInventarioDetalleDTO>>(uri);
    }

    public async Task<TransaccionInventarioDetalleDTO> GetByIdProducto(long idProd)
    {
        var uri = $"{idProd}";
        return await GetAsync<TransaccionInventarioDetalleDTO>(uri);
    }

    public async Task<bool> Update(TransaccionInventarioDTO transaccion)
    {
        var uri = $"";
        return await PutAsync<bool>(uri, transaccion);
    }
}