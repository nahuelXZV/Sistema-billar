using Domain.DTOs.General;

namespace Domain.Interfaces.Services.General;

public interface IDashboardService
{
    Task<DashboardDTO> Get(int mes, int anio);
    Task<DashboardCajeroDTO> GetCajero();
}
