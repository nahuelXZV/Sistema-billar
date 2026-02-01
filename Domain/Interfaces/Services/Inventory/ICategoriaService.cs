using Domain.DTOs.Inventory;

namespace Domain.Interfaces.Services.Inventory;

public interface ICategoriaService
{
    Task<long> Create(CategoriaDTO categoria);
    Task<bool> Update(CategoriaDTO categoria);
    Task<bool> Delete(long id);
    Task<CategoriaDTO> GetById(long id);
    Task<List<CategoriaDTO>> GetAll();
    Task<List<CategoriaDTO>> GetAllSinNivel();
}
