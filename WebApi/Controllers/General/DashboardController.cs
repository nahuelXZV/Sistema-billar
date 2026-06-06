using Application.Features.General.Dashboard.Queries;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.General;

public class DashboardController : MainController
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(await Mediator.Send(new GetDashboardQuery()));
    }
}
