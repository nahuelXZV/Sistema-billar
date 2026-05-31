using Domain.DTOs.Shared;

namespace Domain.Interfaces.Services.Shared;

public interface IArchivoService
{
    Task<FileUploadResultDTO> UploadAsync(Stream fileStream, string fileName, string? contentType);
}
