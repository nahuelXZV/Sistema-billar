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
        model.Vendedor = await _appServices.VendedorService.GetByUsuario(model.IdUsuarioLoggedIn);
        var categoriasBase = await _appServices.CategoriaService.GetCategoriasBase();
        model.PuntoVenta = PuntoVentaMapper.Create(categoriasBase, model.Vendedor);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> VentaMesas()
    {
        var model = _viewModelFactory.Create<VentaViewModel>();
        model.IncluirBlazorComponents = true;
        model.Vendedor = await _appServices.VendedorService.GetByUsuario(model.IdUsuarioLoggedIn);
        model.Mesas = await _appServices.MesasService.GetAll();
        return View(model);
    }
}
