using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;
using Domain.Interfaces.Services.Inventory;
using WebClient.Services.Implementacion;

namespace WebClient.Services.Inventory;

public class TraspasoInventarioService : AppBaseServices, ITraspasoInventarioService
{
    public TraspasoInventarioService(IHttpClientFactory httpClientFactory, IHttpContextAccessor contextAccessor, ILogger<TraspasoInventarioService> logger)
        : base("api/TraspasoInventario", httpClientFactory, contextAccessor, logger)
    {
    }

    public async Task<ResponseFilterDTO<TraspasoInventarioDTO>> GetAll(FilterDTO? filter)
    {
        return await GetAsync<ResponseFilterDTO<TraspasoInventarioDTO>>(AplicarFiltro(filter));
    }

    public async Task<TraspasoInventarioDTO> GetById(long idTraspaso)
    {
        return await GetAsync<TraspasoInventarioDTO>($"{idTraspaso}");
    }

    public async Task<List<InventarioDTO>> GetInventariosDisponibles(long idAlmacen)
    {
        return await GetAsync<List<InventarioDTO>>($"Almacen/{idAlmacen}/Disponibles");
    }

    public async Task<long> Create(TraspasoInventarioDTO traspaso)
    {
        return await PostAsync<long>(traspaso);
    }

    public async Task<bool> Delete(long idTraspaso)
    {
        return await DeleteAsync<bool>($"Delete/{idTraspaso}");
    }
}
