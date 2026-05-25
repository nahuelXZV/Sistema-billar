using Domain.DTOs.Contact;
using Domain.DTOs.Shared;
using Domain.Interfaces.Services.Contact;
using WebClient.Services.Implementacion;

namespace WebClient.Services.Contact;

public class ClienteService : AppBaseServices, IClienteService
{
    public ClienteService(IHttpClientFactory httpClientFactory, IHttpContextAccessor contextAccessor, ILogger<ClienteService> logger)
        : base("api/Cliente", httpClientFactory, contextAccessor, logger)
    {
    }

    public async Task<long> Create(ClienteDTO cliente)
    {
        var uri = "";
        return await PostAsync<long>(uri, cliente);
    }

    public async Task<bool> Delete(long id)
    {
        var uri = $"Delete/{id}";
        return await DeleteAsync<bool>(uri);
    }

    public async Task<ResponseFilterDTO<ClienteDTO>> GetAll(FilterDTO? filter)
    {
        var uri = AplicarFiltro(filter);
        return await GetAsync<ResponseFilterDTO<ClienteDTO>>(uri);
    }

    public async Task<List<ClienteDTO>> GetAll()
    {
        var uri = "GetAll";
        return await GetAsync<List<ClienteDTO>>(uri);
    }

    public async Task<ClienteDTO> GetById(long id)
    {
        var uri = $"{id}";
        return await GetAsync<ClienteDTO>(uri);
    }

    public async Task<bool> Update(ClienteDTO cliente)
    {
        var uri = "";
        return await PutAsync<bool>(uri, cliente);
    }
}
