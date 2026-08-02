using Domain.DTOs.Purchases;
using Domain.DTOs.Shared;
using Domain.Interfaces.Services.Purchases;
using WebClient.Services.Implementacion;

namespace WebClient.Services.Purchases;

public class CompraService : AppBaseServices, ICompraService
{
    public CompraService(IHttpClientFactory httpClientFactory, IHttpContextAccessor contextAccessor, ILogger<CompraService> logger)
        : base("api/Compra", httpClientFactory, contextAccessor, logger)
    {
    }

    public Task<ResponseFilterDTO<CompraDTO>> GetAll(FilterDTO? filter) =>
        GetAsync<ResponseFilterDTO<CompraDTO>>(AplicarFiltro(filter));

    public Task<List<CompraDTO>> GetAll() => GetAsync<List<CompraDTO>>("GetAll");

    public Task<CompraDTO> GetById(long idCompra) => GetAsync<CompraDTO>($"{idCompra}");

    public Task<long> Create(CompraDTO compra) => PostAsync<long>("", compra);

    public Task<bool> Anular(long idCompra, string motivo) =>
        PostAsync<bool>($"{idCompra}/Anular", new AnularCompraDTO { Motivo = motivo });
}
