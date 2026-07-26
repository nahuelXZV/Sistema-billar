using Domain.Entities.Inventory;
using Infraestructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Utils;

public class ValidarStockDisponibleParametros
{
    public required long IdAlmacen { get; init; }
    public required IEnumerable<(long IdProducto, long? IdLote, decimal Cantidad)> Detalles { get; init; }
    public required string ContextoAlmacen { get; init; }
}


public static class InventarioUtils
{
    public static async Task ValidarStockDisponibleAsync(IRepository<Inventario> inventarioRepository, ValidarStockDisponibleParametros parametros, CancellationToken cancellationToken)
    {
        var detallesNormalizados = parametros.Detalles.ToList();
        var idsProductos = detallesNormalizados.Select(detalle => detalle.IdProducto).Distinct().ToList();
        var inventarios = await inventarioRepository.Query()
            .Where(inventario => !inventario.Eliminado
                && inventario.IdAlmacen == parametros.IdAlmacen
                && idsProductos.Contains(inventario.IdProducto))
            .ToListAsync(cancellationToken);

        foreach (var detalle in detallesNormalizados)
        {
            var inventario = inventarios.FirstOrDefault(item =>
                item.IdProducto == detalle.IdProducto && item.IdLote == detalle.IdLote);

            var disponible = inventario?.Cantidad - inventario?.Reservado ?? 0;
            if (disponible < (double)detalle.Cantidad)
            {
                throw new InvalidOperationException(
                    $"Stock insuficiente para el producto {detalle.IdProducto} en el almacén {parametros.ContextoAlmacen}. " +
                    $"Disponible: {disponible}; solicitado: {detalle.Cantidad}.");
            }
        }
    }
}
