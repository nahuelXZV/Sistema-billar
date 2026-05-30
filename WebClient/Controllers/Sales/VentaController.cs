using Microsoft.AspNetCore.Mvc;
using WebClient.Models;
using WebClient.Models.Sales;
using WebClient.Services;

namespace WebClient.Controllers.Sales;

public class VentaController : MainController
{
    private readonly ILogger<VentaController> _logger;

    public VentaController(ViewModelFactory viewModelFactory, ILogger<VentaController> logger, IAppServices services)
        : base(viewModelFactory, services)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> VentaDirecta()
    {
        var model = _viewModelFactory.Create<VentaViewModel>();
        model.IncluirBlazorComponents = true;
        return View(model);
    }
}
