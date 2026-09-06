using Domain.DTOs.Sales;
using Domain.Entities.Sales;
using DomainUtils = Domain.Utils.Utils;
using static Domain.Constants.Constantes;

namespace Application.Common.Utils;

public static class OrdenMesaUtils
{
    public static OrdenMesaDTO Mapear(OrdenVenta orden, UsoMesa usoMesa, IEnumerable<OrdenVentaDetalle> detalles)
    {
        return new OrdenMesaDTO
        {
            Id = orden.Id,
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
                IdCliente = detalle.IdCliente,
                IdProducto = detalle.IdProducto,
                IdProductoConversion = detalle.IdProductoConversion,
                NombreProducto = detalle.NombreProducto,
                NombreUnidadMedida = detalle.NombreUnidadMedida,
                AbreviaturaUnidadMedida = detalle.AbreviaturaUnidadMedida,
                FactorConversion = detalle.FactorConversion,
                Cantidad = detalle.Cantidad,
                PrecioUnitario = detalle.PrecioUnitario,
                Descuento = detalle.Descuento,
                SubTotal = detalle.SubTotal,
                Total = detalle.Total,
                EsTiempoMesa = detalle.IdUsoMesa.HasValue
            }).ToList()
        };
    }

    public static List<OrdenVentaDetalle> CrearDetalles(OrdenMesaDTO solicitud, long idOrdenVenta, long idUsoMesa)
    {
        return solicitud.Detalles.Select(detalle =>
        {
            var subTotal = DomainUtils.Redondear(detalle.Cantidad * detalle.PrecioUnitario);
            if (detalle.Descuento > subTotal) throw new InvalidOperationException("El descuento no puede superar el subtotal del detalle.");

            return new OrdenVentaDetalle
            {
                IdOrdenVenta = idOrdenVenta,
                IdCliente = detalle.IdCliente ?? solicitud.IdCliente,
                IdProducto = detalle.IdProducto,
                IdProductoConversion = detalle.IdProductoConversion,
                IdUsoMesa = detalle.EsTiempoMesa ? idUsoMesa : null,
                IdVendedor = solicitud.IdVendedor,
                NombreProducto = detalle.NombreProducto,
                NombreUnidadMedida = detalle.NombreUnidadMedida,
                AbreviaturaUnidadMedida = detalle.AbreviaturaUnidadMedida,
                FactorConversion = detalle.FactorConversion,
                Cantidad = detalle.Cantidad,
                PrecioUnitario = detalle.PrecioUnitario,
                Descuento = detalle.Descuento,
                SubTotal = subTotal,
                Total = DomainUtils.Redondear(subTotal - detalle.Descuento),
                Estado = (short)EstadoOrdenVenta.Abierta
            };
        }).ToList();
    }

    public static OrdenVenta MapearOrden(OrdenMesaDTO ordenMesa)
    {
        OrdenVenta orden = new OrdenVenta
        {
            IdCliente = ordenMesa.IdCliente,
            Estado = (short)EstadoOrdenVenta.Abierta,
            FechaApertura = DateTime.Now,
            Observacion = ordenMesa.Observacion
        };

        CalcularTotales(orden, ordenMesa);
        return orden;
    }

    public static void CalcularTotales(OrdenVenta orden, OrdenMesaDTO solicitud)
    {
        var subTotalProductos = solicitud.Detalles.Where(detalle => !detalle.EsTiempoMesa)
            .Sum(detalle => DomainUtils.Redondear(detalle.Cantidad * detalle.PrecioUnitario - detalle.Descuento));

        var subTotalTiempo = solicitud.Detalles.Where(detalle => detalle.EsTiempoMesa)
            .Sum(detalle => DomainUtils.Redondear(detalle.Cantidad * detalle.PrecioUnitario - detalle.Descuento));

        var totalAntesDescuento = DomainUtils.Redondear(subTotalProductos + subTotalTiempo);
        if (solicitud.DescuentoGlobal > totalAntesDescuento)
            throw new InvalidOperationException("El descuento no puede superar el subtotal de la orden.");

        orden.SubTotalProductos = DomainUtils.Redondear(subTotalProductos);
        orden.SubTotalTiempo = DomainUtils.Redondear(subTotalTiempo);
        orden.DescuentoGlobal = DomainUtils.Redondear(solicitud.DescuentoGlobal);
        orden.RecargoGlobal = DomainUtils.Redondear(solicitud.RecargoGlobal);
        orden.Total = DomainUtils.Redondear(totalAntesDescuento - orden.DescuentoGlobal + orden.RecargoGlobal);
        orden.SaldoPendiente = orden.Total;
    }

    public static void ActualizarTiempo(UsoMesa usoMesa, DateTime fechaActual)
    {
        if (usoMesa.Estado != (short)EstadoUsoMesa.EnCurso) return;

        double minutosConsumidos = Math.Max(0, (fechaActual - usoMesa.FechaInicio).TotalMinutes);
        usoMesa.MinutosConsumidos = (decimal)minutosConsumidos;
        usoMesa.MontoCalculado = usoMesa.MinutosConsumidos / 60 * usoMesa.TarifaAplicada;
    }
}
