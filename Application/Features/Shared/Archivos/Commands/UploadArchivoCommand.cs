using Application.Interfaces;
using Domain.Common;
using Domain.DTOs.Shared;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Shared.Archivos.Commands;

public class UploadArchivoCommand : ICommand<Response<FileUploadResultDTO>>
{
    public const long MaxFileSize = 10 * 1024 * 1024;

    public required IFormFile Archivo { get; set; }
    public string? WebRootPath { get; set; }
    public string ContentRootPath { get; set; } = string.Empty;
    public string RequestScheme { get; set; } = string.Empty;
    public string RequestHost { get; set; } = string.Empty;
    public string RequestPathBase { get; set; } = string.Empty;
}

public class UploadArchivoCommandHandler : ICommandHandler<UploadArchivoCommand, Response<FileUploadResultDTO>>
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".csv"
    };

    public async Task<Response<FileUploadResultDTO>> Handle(UploadArchivoCommand request, CancellationToken cancellationToken)
    {
        if (request.Archivo is null || request.Archivo.Length == 0)
        {
            return new Response<FileUploadResultDTO>("Debe enviar un archivo valido.");
        }

        if (request.Archivo.Length > UploadArchivoCommand.MaxFileSize)
        {
            return new Response<FileUploadResultDTO>($"El archivo no puede superar {UploadArchivoCommand.MaxFileSize / 1024 / 1024} MB.");
        }

        var originalFileName = Path.GetFileName(request.Archivo.FileName);
        var extension = Path.GetExtension(originalFileName);

        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            return new Response<FileUploadResultDTO>("El tipo de archivo no esta permitido.");
        }

        var now = DateTime.UtcNow;
        var webRootPath = request.WebRootPath ?? Path.Combine(request.ContentRootPath, "wwwroot");
        var relativeFolder = Path.Combine("uploads", now.ToString("yyyy"), now.ToString("MM"));
        var uploadFolder = Path.Combine(webRootPath, relativeFolder);

        Directory.CreateDirectory(uploadFolder);

        var storedFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var filePath = Path.Combine(uploadFolder, storedFileName);

        await using (var stream = File.Create(filePath))
        {
            await request.Archivo.CopyToAsync(stream, cancellationToken);
        }

        var urlPath = string.Join('/',
            request.RequestPathBase.TrimEnd('/'),
            relativeFolder.Replace(Path.DirectorySeparatorChar, '/'),
            Uri.EscapeDataString(storedFileName)).TrimStart('/');

        var fileUrl = $"{request.RequestScheme}://{request.RequestHost}/{urlPath}";

        return new Response<FileUploadResultDTO>(new FileUploadResultDTO
        {
            Url = fileUrl,
            FileName = storedFileName,
            OriginalFileName = originalFileName,
            ContentType = request.Archivo.ContentType ?? string.Empty,
            Size = request.Archivo.Length
        });
    }
}
