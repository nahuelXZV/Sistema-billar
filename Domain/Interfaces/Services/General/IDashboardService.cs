using Domain.DTOs.General;

namespace Domain.Interfaces.Services.General;

public interface IDashboardService
{
    Task<DashboardDTO> Get();
    Task<DashboardCajeroDTO> GetCajero();
}
