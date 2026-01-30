using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
public class HomeController
{

    [HttpGet("/")]
    public IActionResult Get() => new OkObjectResult("API is running");

}
