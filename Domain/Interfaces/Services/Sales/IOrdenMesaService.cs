using Domain.DTOs.Sales;

namespace Domain.Interfaces.Services.Sales;

public interface IOrdenMesaService
{
    Task<OrdenMesaDTO> Guardar(OrdenMesaDTO ordenMesa);
    Task<OrdenMesaDTO?> GetByMesa(long idMesa);
    Task<List<OrdenMesaDTO>> GetAbiertas();
    Task<OrdenMesaDTO> IniciarCronometro(long idOrdenVenta);
    Task<OrdenMesaDTO> FinalizarCronometro(long idOrdenVenta);
}
