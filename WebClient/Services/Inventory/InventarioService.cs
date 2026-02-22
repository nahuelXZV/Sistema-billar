using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;
using Domain.Interfaces.Services.Inventory;
using WebClient.Services.Implementacion;

namespace WebClient.Services.Inventory;

public class InventarioService : AppBaseServices, IInventarioService
{
    public InventarioService(IHttpClientFactory httpClientFactory, IHttpContextAccessor contextAccessor, ILogger<InventarioService> logger)
        : base("api/Inventario", httpClientFactory, contextAccessor, logger)
    {
    }

    public async Task<ResponseFilterDTO<InventarioDTO>> GetAll(FilterDTO? filter)
    {
        var uri = AplicarFiltro(filter);
        return await GetAsync<ResponseFilterDTO<InventarioDTO>>(uri);
    }

    public async Task<InventarioDTO> GetByIdProducto(long idProd)
    {
        var uri = $"{idProd}";
        return await GetAsync<InventarioDTO>(uri);
    }

}
