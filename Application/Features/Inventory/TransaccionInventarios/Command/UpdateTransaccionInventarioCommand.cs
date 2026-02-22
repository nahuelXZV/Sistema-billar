using Application.Interfaces;
using Domain.Common;
using Domain.DTOs.Inventory;

namespace Application.Features.Inventory.TransaccionInventarios.Command;

public class UpdateTransaccionInventarioCommand : ICommand<Response<long>>
{
    public required TransaccionInventarioDTO TransaccionInventarioDTO { get; set; }
}