using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;
using WebClient.Extensions;
using WebClient.Models.Inventory;
using WebClient.Models;
using System.Globalization;
using WebClient.Services;

namespace WebClient.Controllers.Inventory;

public class CategoriaController : MainController
{
    private readonly ILogger<CategoriaController> _logger;

    public CategoriaController(ViewModelFactory viewModelFactory, ILogger<CategoriaController> logger, IAppServices services)
        : base(viewModelFactory, services)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Listado()
    {
        var model = _viewModelFactory.Create<CategoriaViewModel>();
        model.IncluirBlazorComponents = true;
        model.ListaCategorias = await _appServices.CategoriaService.GetAll();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Eliminar([FromForm] long id)
    {
        try
        {
            await _appServices.CategoriaService.Delete(id);
            this.AddSuccessTempMessage("Categoria eliminado correctamente");
        }
        catch (Exception ex)
        {
            this.AddTempMessage(ex);
        }
        return RedirectToAction("Listado");
    }
}