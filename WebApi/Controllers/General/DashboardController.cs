using Application.Features.General.Dashboard.Queries;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.General;

public class DashboardController : MainController
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int? mes, [FromQuery] int? anio)
    {
        return Ok(await Mediator.Send(new GetDashboardQuery
        {
            Mes = mes,
            Anio = anio
        }));
    }

    [HttpGet("Cajero")]
    public async Task<IActionResult> GetCajero()
    {
        return Ok(await Mediator.Send(new GetDashboardCajeroQuery
        {
            IdUsuario = IdUsuarioActual
        }));
    }
}
