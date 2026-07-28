using Domain.DTOs.Contact;
using Domain.DTOs.Shared;
using Domain.Interfaces.Services.Contact;
using WebClient.Services.Implementacion;

namespace WebClient.Services.Contact;

public class ProveedorService : AppBaseServices, IProveedorService
{
    public ProveedorService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor contextAccessor,
        ILogger<ProveedorService> logger)
        : base("api/Proveedor", httpClientFactory, contextAccessor, logger)
    {
    }

    public Task<long> Create(ProveedorDTO proveedor) => PostAsync<long>("", proveedor);

    public Task<bool> Delete(long id) => DeleteAsync<bool>($"Delete/{id}");

    public Task<ResponseFilterDTO<ProveedorDTO>> GetAll(FilterDTO? filter) =>
        GetAsync<ResponseFilterDTO<ProveedorDTO>>(AplicarFiltro(filter));

    public Task<List<ProveedorDTO>> GetAll() => GetAsync<List<ProveedorDTO>>("GetAll");

    public Task<ProveedorDTO> GetById(long id) => GetAsync<ProveedorDTO>($"{id}");

    public Task<bool> Update(ProveedorDTO proveedor) => PutAsync<bool>("", proveedor);
}
