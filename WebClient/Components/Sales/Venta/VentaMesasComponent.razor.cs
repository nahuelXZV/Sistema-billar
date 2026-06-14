using Domain.DTOs.Configuration;
using Domain.DTOs.Sales;
using Microsoft.AspNetCore.Components;
using System.Text.Json;
using WebClient.Models.Sales;
using static Domain.Constants.Constantes;

namespace WebClient.Components.Sales.Venta;

public partial class VentaMesasComponent
{
    [Parameter] public VentaViewModel Model { get; set; } = new();

    private readonly Dictionary<long, OrdenMesaDTO> _ordenesPorMesa = [];
    private MesaDTO? MesaSeleccionada { get; set; }
    private VentaViewModel? ModeloVentaSeleccionada { get; set; }
    private bool GuardandoOrden { get; set; }
    private bool MostrarConfirmacionVolver { get; set; }
    private string _firmaOrdenGuardada = string.Empty;

    private int CantidadMesasActivas => Model.Mesas.Count(mesa => mesa.Activo);
    private int CantidadMesasInactivas => Model.Mesas.Count(mesa => !mesa.Activo);
    private string EtiquetaContextoVenta => MesaSeleccionada is null ? "Venta única" : "Mesa seleccionada";
    private string TituloContextoVenta => MesaSeleccionada?.Nombre ?? "Venta directa";

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        var ordenesAbiertas = await AppServices.OrdenMesaService.GetAbiertas();
        foreach (var orden in ordenesAbiertas)
        {
            _ordenesPorMesa[orden.IdMesa] = orden;
        }
    }


    #region Seleccionar Mesa
    private async Task SeleccionarMesaHandler(MesaDTO mesa)
    {
        if (!mesa.Activo)
        {
            return;
        }

        MesaSeleccionada = mesa;
        var modelo = CrearVentaViewModel();

        if (!_ordenesPorMesa.TryGetValue(mesa.Id, out var ordenMesa))
        {
            ordenMesa = await AppServices.OrdenMesaService.GetByMesa(mesa.Id);
        }

        if (ordenMesa is null)
        {
            ordenMesa = CrearOrdenNueva(mesa, modelo);
        }
        else
        {
            _ordenesPorMesa[mesa.Id] = ordenMesa;
            await RestaurarOrdenAsync(modelo, ordenMesa);
        }

        modelo.OrdenMesa = ordenMesa;
        modelo.PuntoVenta.IdMesa = mesa.Id;
        modelo.PuntoVenta.IdOrdenVenta = ordenMesa.Id > 0 ? ordenMesa.Id : null;
        modelo.PuntoVenta.IdUsoMesa = ordenMesa.IdUsoMesa > 0 ? ordenMesa.IdUsoMesa : null;
        ModeloVentaSeleccionada = modelo;
        _firmaOrdenGuardada = CrearFirmaOrden(modelo);
    }

    private void AbrirVentaDirecta()
    {
        MesaSeleccionada = null;
        ModeloVentaSeleccionada = CrearVentaViewModel();
        _firmaOrdenGuardada = CrearFirmaOrden(ModeloVentaSeleccionada);
    }

    private VentaViewModel CrearVentaViewModel()
    {
        var puntoVentaBase = Model.PuntoVenta;
        var puntoVenta = new PuntoVentaViewModel
        {
            IdVendedor = puntoVentaBase.IdVendedor,
            IdListaPrecio = puntoVentaBase.IdListaPrecio,
            NombreVendedor = puntoVentaBase.NombreVendedor,
            ClienteSeleccionado = puntoVentaBase.ClienteSeleccionado,
            RootCategories = puntoVentaBase.RootCategories
        };

        return new VentaViewModel
        {
            Vendedor = Model.Vendedor,
            PuntoVenta = puntoVenta
        };
    }

    private OrdenMesaDTO CrearOrdenNueva(MesaDTO mesa, VentaViewModel modelo)
    {
        var ahora = DateTime.Now;
        return new OrdenMesaDTO
        {
            IdMesa = mesa.Id,
            IdCliente = modelo.PuntoVenta.ClienteSeleccionado?.Id,
            IdVendedor = modelo.Vendedor?.Id ?? 0,
            EstadoOrden = (short)EstadoOrdenVenta.Abierta,
            EstadoUsoMesa = (short)EstadoUsoMesa.Pendiente,
            FechaApertura = ahora,
            FechaInicio = ahora
        };
    }

    private async Task RestaurarOrdenAsync(VentaViewModel modelo, OrdenMesaDTO ordenMesa)
    {
        var puntoVenta = modelo.PuntoVenta;
        puntoVenta.IdOrdenVenta = ordenMesa.Id;
        puntoVenta.IdUsoMesa = ordenMesa.IdUsoMesa;
        puntoVenta.IdMesa = ordenMesa.IdMesa;
        puntoVenta.DescuentoGlobal = ordenMesa.DescuentoGlobal;
        puntoVenta.RecargoGlobal = ordenMesa.RecargoGlobal;
        puntoVenta.NotaVenta = ordenMesa.Observacion;

        if (ordenMesa.IdCliente.HasValue)
        {
            puntoVenta.ClienteSeleccionado = await AppServices.ClienteService.GetById(ordenMesa.IdCliente.Value);
        }

        puntoVenta.DetalleItems = ordenMesa.Detalles.Select(detalle => new ItemsViewModel
        {
            IdOrdenVentaDetalle = detalle.Id,
            IdProducto = detalle.IdProducto,
            Nombre = detalle.NombreProducto,
            Cantidad = detalle.Cantidad,
            PrecioUnitario = detalle.PrecioUnitario,
            EsTiempoMesa = detalle.EsTiempoMesa
        }).ToList();
    }
    #endregion

    #region Guardar orden y preparar pago
    private async Task GuardarOrdenMesaAsync()
    {
        if (MesaSeleccionada is null || ModeloVentaSeleccionada is null || GuardandoOrden)
        {
            return;
        }

        GuardandoOrden = true;
        try
        {
            var solicitud = ConstruirOrdenMesa();
            var ordenGuardada = await AppServices.OrdenMesaService.Guardar(solicitud);

            ModeloVentaSeleccionada.OrdenMesa = ordenGuardada;
            ModeloVentaSeleccionada.PuntoVenta.IdOrdenVenta = ordenGuardada.Id;
            ModeloVentaSeleccionada.PuntoVenta.IdUsoMesa = ordenGuardada.IdUsoMesa;
            ActualizarIdsDetalles(ordenGuardada);
            _ordenesPorMesa[MesaSeleccionada.Id] = ordenGuardada;
            _firmaOrdenGuardada = CrearFirmaOrden(ModeloVentaSeleccionada);
        }
        finally
        {
            GuardandoOrden = false;
        }
    }

    private OrdenMesaDTO ConstruirOrdenMesa()
    {
        var modelo = ModeloVentaSeleccionada ?? throw new InvalidOperationException("No existe una venta de mesa seleccionada.");
        var mesa = MesaSeleccionada ?? throw new InvalidOperationException("No existe una mesa seleccionada.");
        var ordenActual = modelo.OrdenMesa ?? CrearOrdenNueva(mesa, modelo);
        var puntoVenta = modelo.PuntoVenta;

        return new OrdenMesaDTO
        {
            Id = ordenActual.Id,
            IdUsoMesa = ordenActual.IdUsoMesa,
            IdMesa = mesa.Id,
            IdCliente = puntoVenta.ClienteSeleccionado?.Id,
            IdVendedor = modelo.Vendedor?.Id ?? 0,
            Numero = ordenActual.Numero,
            EstadoOrden = ordenActual.EstadoOrden,
            EstadoUsoMesa = ordenActual.EstadoUsoMesa,
            FechaApertura = ordenActual.FechaApertura,
            FechaInicio = ordenActual.FechaInicio,
            MinutosConsumidos = ordenActual.MinutosConsumidos,
            TarifaAplicada = ordenActual.TarifaAplicada,
            MontoCalculado = ordenActual.MontoCalculado,
            DescuentoGlobal = puntoVenta.DescuentoGlobal,
            RecargoGlobal = puntoVenta.RecargoGlobal,
            Observacion = puntoVenta.NotaVenta,
            Detalles = puntoVenta.DetalleItems.Select(detalleVenta => new OrdenMesaDetalleDTO
            {
                Id = detalleVenta.IdOrdenVentaDetalle ?? 0,
                IdProducto = detalleVenta.IdProducto,
                NombreProducto = detalleVenta.Nombre,
                Cantidad = detalleVenta.Cantidad,
                PrecioUnitario = detalleVenta.PrecioUnitario,
                Descuento = 0,
                SubTotal = detalleVenta.Total,
                Total = detalleVenta.Total,
                EsTiempoMesa = detalleVenta.EsTiempoMesa
            }).ToList()
        };
    }

    private void ActualizarIdsDetalles(OrdenMesaDTO ordenGuardada)
    {
        if (ModeloVentaSeleccionada is null)
        {
            return;
        }

        foreach (var detalleVenta in ModeloVentaSeleccionada.PuntoVenta.DetalleItems)
        {
            var detalle = ordenGuardada.Detalles.FirstOrDefault(detalle =>
                detalle.IdProducto == detalleVenta.IdProducto &&
                detalle.EsTiempoMesa == detalleVenta.EsTiempoMesa);

            detalleVenta.IdOrdenVentaDetalle = detalle?.Id;
        }

        foreach (var itemPago in ModeloVentaSeleccionada.PuntoVenta.ProductosPagar)
        {
            var detalle = ordenGuardada.Detalles.FirstOrDefault(detalle =>
                detalle.IdProducto == itemPago.IdProducto &&
                detalle.EsTiempoMesa == itemPago.EsTiempoMesa);

            itemPago.IdOrdenVentaDetalle = detalle?.Id;
        }
    }

    private async Task PrepararPagoAsync()
    {
        if (ModeloVentaSeleccionada is null)
        {
            return;
        }

        foreach (var itemPago in ModeloVentaSeleccionada.PuntoVenta.ProductosPagar.Where(detallePago => detallePago.IsSelected))
        {
            var itemVenta = ModeloVentaSeleccionada.PuntoVenta.DetalleItems.FirstOrDefault(detalleVenta =>
                detalleVenta.IdProducto == itemPago.IdProducto &&
                detalleVenta.EsTiempoMesa == itemPago.EsTiempoMesa);

            if (itemVenta is not null)
            {
                if (itemVenta.EsTiempoMesa)
                {
                    itemPago.CantidadPagar = itemVenta.Cantidad;
                    itemPago.CantidadDisponible = itemVenta.Cantidad;
                }
            }
        }

        await GuardarOrdenMesaAsync();
    }

    private async Task VentaFinalizadaAsync(long idVenta)
    {
        if (ModeloVentaSeleccionada?.OrdenMesa is null || MesaSeleccionada is null)
        {
            return;
        }

        var ordenPendiente = await AppServices.OrdenMesaService.GetByMesa(MesaSeleccionada.Id);
        if (ordenPendiente is null)
        {
            _ordenesPorMesa.Remove(MesaSeleccionada.Id);
            VolverAMesasConfirmado();
            return;
        }

        await RestaurarOrdenAsync(ModeloVentaSeleccionada, ordenPendiente);
        ModeloVentaSeleccionada.OrdenMesa = ordenPendiente;
        _ordenesPorMesa[MesaSeleccionada.Id] = ordenPendiente;
        _firmaOrdenGuardada = CrearFirmaOrden(ModeloVentaSeleccionada);
    }

    private void OrdenActualizada(OrdenMesaDTO ordenMesa)
    {
        if (ModeloVentaSeleccionada is null) return;

        ModeloVentaSeleccionada.OrdenMesa = ordenMesa;
        _ordenesPorMesa[ordenMesa.IdMesa] = ordenMesa;
    }
    #endregion

    #region Volver a mesas
    private void SolicitarVolverAMesas()
    {
        if (HayCambiosSinGuardar())
        {
            MostrarConfirmacionVolver = true;
            return;
        }

        VolverAMesasConfirmado();
    }

    private void CancelarVolverAMesas()
    {
        MostrarConfirmacionVolver = false;
    }

    private void ConfirmarVolverAMesas()
    {
        MostrarConfirmacionVolver = false;
        VolverAMesasConfirmado();
    }

    private void VolverAMesasConfirmado()
    {
        MesaSeleccionada = null;
        ModeloVentaSeleccionada = null;
        _firmaOrdenGuardada = string.Empty;
    }
    #endregion

    #region Firma de cambios
    private bool HayCambiosSinGuardar()
    {
        return ModeloVentaSeleccionada is not null && _firmaOrdenGuardada != CrearFirmaOrden(ModeloVentaSeleccionada);
    }

    private static string CrearFirmaOrden(VentaViewModel? modelo)
    {
        if (modelo is null)
        {
            return string.Empty;
        }

        var puntoVenta = modelo.PuntoVenta;
        var estado = new
        {
            IdCliente = puntoVenta.ClienteSeleccionado?.Id,
            puntoVenta.NotaVenta,
            puntoVenta.DescuentoGlobal,
            puntoVenta.RecargoGlobal,
            Detalles = puntoVenta.DetalleItems
                .OrderBy(detalle => detalle.EsTiempoMesa)
                .ThenBy(detalle => detalle.IdProducto)
                .Select(detalle => new
                {
                    detalle.IdProducto,
                    detalle.Nombre,
                    detalle.Cantidad,
                    detalle.PrecioUnitario,
                    detalle.EsTiempoMesa
                })
                .ToList()
        };

        return JsonSerializer.Serialize(estado);
    }
    #endregion

    #region Utils
    private string ObtenerEstadoMesa(MesaDTO mesa)
    {
        if (!mesa.Activo)
        {
            return "Inactiva";
        }

        return _ordenesPorMesa.ContainsKey(mesa.Id) ? "Ocupada" : "Libre";
    }

    private string ObtenerAccionMesa(MesaDTO mesa)
    {
        if (!mesa.Activo)
        {
            return "No disponible";
        }

        return _ordenesPorMesa.ContainsKey(mesa.Id) ? "Continuar orden" : "Abrir venta";
    }

    private string ObtenerClaseMesa(MesaDTO mesa)
    {
        if (!mesa.Activo)
        {
            return "is-disabled";
        }

        return _ordenesPorMesa.ContainsKey(mesa.Id) ? "is-busy" : "is-free";
    }
    #endregion
}

