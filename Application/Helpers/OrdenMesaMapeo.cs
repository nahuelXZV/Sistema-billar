using Domain.DTOs.Sales;
using Domain.Entities.Sales;

namespace Application.Helpers;

internal static class OrdenMesaMapeo
{
    public static OrdenMesaDTO Crear(OrdenVenta orden, UsoMesa usoMesa, IEnumerable<OrdenVentaDetalle> detalles)
    {
        return new OrdenMesaDTO
        {
            IdOrdenVenta = orden.Id,
            IdUsoMesa = usoMesa.Id,
            IdMesa = usoMesa.IdMesa,
            IdCliente = orden.IdCliente,
            IdVendedor = detalles.FirstOrDefault()?.IdVendedor ?? 0,
            Numero = orden.Numero,
            EstadoOrden = orden.Estado,
            EstadoUsoMesa = usoMesa.Estado,
            FechaApertura = orden.FechaApertura,
            FechaInicio = usoMesa.FechaInicio,
            FechaFin = usoMesa.FechaFin,
            MinutosConsumidos = usoMesa.MinutosConsumidos,
            TarifaAplicada = Convert.ToDecimal(usoMesa.TarifaAplicada),
            MontoCalculado = Convert.ToDecimal(usoMesa.MontoCalculado),
            DescuentoGlobal = orden.DescuentoGlobal,
            RecargoGlobal = orden.RecargoGlobal,
            Total = orden.Total,
            Observacion = orden.Observacion ?? string.Empty,
            Detalles = detalles.Select(detalle => new OrdenMesaDetalleDTO
            {
                Id = detalle.Id,
                IdProducto = detalle.IdProducto,
                NombreProducto = detalle.NombreProducto,
                Cantidad = detalle.Cantidad,
                PrecioUnitario = detalle.PrecioUnitario,
                Descuento = detalle.Descuento,
                SubTotal = detalle.SubTotal,
                Total = detalle.Total,
                EsTiempoMesa = detalle.IdUsoMesa.HasValue
            }).ToList()
        };
    }
}
