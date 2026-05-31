using Application.Features.Shared.Archivos.Commands;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

public class ArchivoController : MainController
{
    private readonly IWebHostEnvironment _environment;

    public ArchivoController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpPost]
    [RequestSizeLimit(UploadArchivoCommand.MaxFileSize)]
    public async Task<IActionResult> Upload(IFormFile archivo, CancellationToken cancellationToken)
    {
        return Ok(await Mediator.Send(new UploadArchivoCommand
        {
            Archivo = archivo,
            WebRootPath = _environment.WebRootPath,
            ContentRootPath = _environment.ContentRootPath,
            RequestScheme = Request.Scheme,
            RequestHost = Request.Host.ToString(),
            RequestPathBase = Request.PathBase.ToString()
        }, cancellationToken));
    }
}
