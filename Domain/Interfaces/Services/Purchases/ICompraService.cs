using Domain.DTOs.Purchases;
using Domain.DTOs.Shared;

namespace Domain.Interfaces.Services.Purchases;

public interface ICompraService
{
    Task<ResponseFilterDTO<CompraDTO>> GetAll(FilterDTO? filter);
    Task<List<CompraDTO>> GetAll();
    Task<CompraDTO> GetById(long idCompra);
    Task<long> Create(CompraDTO compra);
    Task<bool> Anular(long idCompra, string motivo);
}
