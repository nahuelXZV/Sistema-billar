using Application.Features.Inventory.Categorias.Commands;
using Application.Features.Inventory.Categorias.Queries;
using Domain.DTOs.Inventory;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Inventory;

public class CategoriaController : MainController
{
    private readonly ILogger<CategoriaController> _logger;

    public CategoriaController(ILogger<CategoriaController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(await Mediator.Send(new GetCategoriasQuery() { }));
    }

    [HttpGet("SinNivel")]
    public async Task<IActionResult> GetSinNivel()
    {
        return Ok(await Mediator.Send(new GetCategoriasSinNivelQuery() { }));
    }

    [HttpGet("{idCat}")]
    public async Task<IActionResult> GetById(long idCat)
    {
        return Ok(await Mediator.Send(new GetCategoriaByIdQuery() { Id = idCat }));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CategoriaDTO categoriaDTO)
    {
        return Ok(await Mediator.Send(new CreateCategoriaCommand { CategoriaDTO = categoriaDTO }));
    }

    [HttpPut]
    public async Task<IActionResult> Update(CategoriaDTO categoriaDTO)
    {
        return Ok(await Mediator.Send(new UpdateCategoriaCommand { CategoriaDTO = categoriaDTO }));
    }

    [HttpDelete("Delete/{idCat}")]
    public async Task<IActionResult> Delete(long idCat)
    {
        return Ok(await Mediator.Send(new DeleteCategoriaCommand { CategoriaId = idCat }));
    }
}
