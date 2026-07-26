using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;

namespace Domain.Interfaces.Services.Inventory;

public interface ITraspasoInventarioService
{
    Task<ResponseFilterDTO<TraspasoInventarioDTO>> GetAll(FilterDTO? filter);
    Task<TraspasoInventarioDTO> GetById(long idTraspaso);
    Task<List<InventarioDTO>> GetInventariosDisponibles(long idAlmacen);
    Task<long> Create(TraspasoInventarioDTO traspaso);
    Task<bool> Delete(long idTraspaso);
}
