using Application.Features.Inventory.Productos.Commands;
using Application.Features.Inventory.Productos.Queries;
using Application.Features.Inventory.UnidadesMedidas.Queries;
using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Inventory;

public class ProductoController : MainController
{
    private readonly ILogger<ProductoController> _logger;

    public ProductoController(ILogger<ProductoController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] FilterDTO? filter)
    {
        return Ok(await Mediator.Send(new GetProductosFilterQuery() { Filter = filter }));
    }


    [HttpGet("GetAll")]
    public async Task<IActionResult> Get()
    {
        return Ok(await Mediator.Send(new GetProductosQuery() { }));
    }

    [HttpGet("{idProd}")]
    public async Task<IActionResult> GetById(long idProd)
    {
        return Ok(await Mediator.Send(new GetProductoByIdQuery() { Id = idProd }));
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProductoDTO productoDto)
    {
        return Ok(await Mediator.Send(new CreateProductoCommand { ProductoDTO = productoDto }));
    }

    [HttpPut]
    public async Task<IActionResult> Update(ProductoDTO productoDto)
    {
        return Ok(await Mediator.Send(new UpdateProductoCommand { ProductoDTO = productoDto }));
    }

    [HttpDelete("Delete/{idProd}")]
    public async Task<IActionResult> Delete(long idProd)
    {
        return Ok(await Mediator.Send(new DeleteProductoCommand { ProductoId = idProd }));
    }
}
