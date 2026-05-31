using System.Net.Http.Headers;
using Domain.DTOs.Shared;
using Domain.Interfaces.Services.Shared;
using WebClient.Services.Implementacion;

namespace WebClient.Services.Shared;

public class ArchivoService : AppBaseServices, IArchivoService
{
    public ArchivoService(IHttpClientFactory httpClientFactory, IHttpContextAccessor contextAccessor, ILogger<ArchivoService> logger)
        : base("api/Archivo", httpClientFactory, contextAccessor, logger)
    {
    }

    public async Task<FileUploadResultDTO> UploadAsync(Stream fileStream, string fileName, string? contentType)
    {
        using var content = new MultipartFormDataContent();
        using var fileContent = new StreamContent(fileStream);

        if (!string.IsNullOrWhiteSpace(contentType))
        {
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
        }

        content.Add(fileContent, "archivo", fileName);

        return await PostMultipartAsync<FileUploadResultDTO>("", content);
    }
}
