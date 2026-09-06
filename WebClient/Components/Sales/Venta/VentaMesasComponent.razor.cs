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
    private bool MostrarCambioMesa { get; set; }
    private bool MostrarEliminarOrden { get; set; }
    private MesaDTO? MesaAccion { get; set; }
    private OrdenMesaDTO? OrdenAccion { get; set; }
    private List<MesaDTO> MesasDisponiblesCambio { get; set; } = [];
    private long IdMesaDestino { get; set; }
    private bool ProcesandoCambioMesa { get; set; }
    private bool ProcesandoEliminarOrden { get; set; }
    private string _firmaOrdenGuardada = string.Empty;

    private int CantidadMesasActivas => Model.Mesas.Count(mesa => mesa.Activo);
    private int CantidadMesasInactivas => Model.Mesas.Count(mesa => !mesa.Activo);
    private string EtiquetaContextoVenta => MesaSeleccionada is null ? "Venta única" : "Mesa seleccionada";
    private string TituloContextoVenta => MesaSeleccionada?.Nombre ?? "Venta directa";
    private string NombreMesaDestino => MesasDisponiblesCambio.FirstOrDefault(mesa => mesa.Id == IdMesaDestino)?.Nombre ?? "Mesa destino";

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
            IdClienteDefault = puntoVentaBase.IdClienteDefault,
            ClienteSeleccionado = puntoVentaBase.ClienteSeleccionado,
            Clientes = puntoVentaBase.Clientes,
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
            IdCliente = detalle.IdCliente ?? puntoVenta.IdClienteDefault,
            IdProducto = detalle.IdProducto,
            IdProductoConversion = detalle.IdProductoConversion,
            Nombre = detalle.NombreProducto,
            NombreUnidadMedida = detalle.NombreUnidadMedida,
            AbreviaturaUnidadMedida = detalle.AbreviaturaUnidadMedida,
            FactorConversion = detalle.FactorConversion,
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
                IdCliente = detalleVenta.IdCliente,
                IdProducto = detalleVenta.IdProducto,
                IdProductoConversion = detalleVenta.IdProductoConversion,
                NombreProducto = detalleVenta.Nombre,
                NombreUnidadMedida = detalleVenta.NombreUnidadMedida,
                AbreviaturaUnidadMedida = detalleVenta.AbreviaturaUnidadMedida,
                FactorConversion = detalleVenta.FactorConversion,
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
                detalle.IdProductoConversion == detalleVenta.IdProductoConversion &&
                detalle.IdCliente == detalleVenta.IdCliente &&
                detalle.EsTiempoMesa == detalleVenta.EsTiempoMesa);

            detalleVenta.IdOrdenVentaDetalle = detalle?.Id;
        }

        foreach (var itemPago in ModeloVentaSeleccionada.PuntoVenta.ProductosPagar)
        {
            var detalle = ordenGuardada.Detalles.FirstOrDefault(detalle =>
                detalle.IdProducto == itemPago.IdProducto &&
                detalle.IdProductoConversion == itemPago.IdProductoConversion &&
                detalle.IdCliente == itemPago.IdCliente &&
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
                detalleVenta.IdOrdenVentaDetalle == itemPago.IdOrdenVentaDetalle ||
                (detalleVenta.IdProducto == itemPago.IdProducto &&
                 detalleVenta.IdProductoConversion == itemPago.IdProductoConversion &&
                 detalleVenta.IdCliente == itemPago.IdCliente &&
                 detalleVenta.EsTiempoMesa == itemPago.EsTiempoMesa));

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
                // El temporizador actualiza este detalle cada segundo; no representa
                // una modificación manual pendiente de guardar.
                .Where(detalle => !detalle.EsTiempoMesa)
                .OrderBy(detalle => detalle.IdProducto)
                .ThenBy(detalle => detalle.IdProductoConversion)
                .Select(detalle => new
                {
                    detalle.IdProducto,
                    detalle.IdProductoConversion,
                    detalle.IdCliente,
                    detalle.Nombre,
                    detalle.Cantidad,
                    detalle.PrecioUnitario
                })
                .ToList(),
            DetallesTiempo = puntoVenta.DetalleItems
                .Where(detalle => detalle.EsTiempoMesa)
                .Select(detalle => new
                {
                    detalle.IdProducto,
                    detalle.IdProductoConversion,
                    detalle.IdCliente
                })
                .ToList()
        };

        return JsonSerializer.Serialize(estado);
    }
    #endregion

    #region Acciones de orden desde la tarjeta
    private void SolicitarCambiarMesa(MesaDTO mesa, OrdenMesaDTO ordenMesa)
    {
        MesaAccion = mesa;
        OrdenAccion = ordenMesa;
        IdMesaDestino = 0;
        MesasDisponiblesCambio = Model.Mesas
            .Where(mesaDisponible =>
                mesaDisponible.Activo &&
                mesaDisponible.Id != mesa.Id &&
                !_ordenesPorMesa.ContainsKey(mesaDisponible.Id))
            .OrderBy(mesaDisponible => mesaDisponible.Nombre)
            .ToList();
        MostrarCambioMesa = true;
    }

    private void CancelarCambioMesa()
    {
        if (ProcesandoCambioMesa)
        {
            return;
        }

        MostrarCambioMesa = false;
        LimpiarAccionMesa();
    }

    private async Task ConfirmarCambioMesaAsync()
    {
        if (MesaAccion is null || OrdenAccion is null || IdMesaDestino <= 0 || ProcesandoCambioMesa)
        {
            return;
        }

        ProcesandoCambioMesa = true;
        try
        {
            var idMesaOrigen = MesaAccion.Id;
            var idMesaDestino = IdMesaDestino;
            var nombreMesaDestino = NombreMesaDestino;
            var ordenTransferida = await AppServices.OrdenMesaService.Transferir(new TransferirOrdenMesaDTO
            {
                IdOrdenVenta = OrdenAccion.Id,
                IdMesaDestino = idMesaDestino
            });

            _ordenesPorMesa.Remove(idMesaOrigen);
            _ordenesPorMesa[idMesaDestino] = ordenTransferida;
            MostrarCambioMesa = false;
            LimpiarAccionMesa();
            await ShowSuccessMessage($"Orden transferida a {nombreMesaDestino}.");
        }
        catch (Exception ex)
        {
            await ShowErrorMessage(ex);
        }
        finally
        {
            ProcesandoCambioMesa = false;
        }
    }

    private void SolicitarEliminarOrden(MesaDTO mesa, OrdenMesaDTO ordenMesa)
    {
        MesaAccion = mesa;
        OrdenAccion = ordenMesa;
        MostrarEliminarOrden = true;
    }

    private void CancelarEliminarOrden()
    {
        if (ProcesandoEliminarOrden)
        {
            return;
        }

        MostrarEliminarOrden = false;
        LimpiarAccionMesa();
    }

    private async Task ConfirmarEliminarOrdenAsync()
    {
        if (MesaAccion is null || OrdenAccion is null || ProcesandoEliminarOrden)
        {
            return;
        }

        ProcesandoEliminarOrden = true;
        try
        {
            var idMesa = MesaAccion.Id;
            var resultado = await AppServices.OrdenMesaService.Eliminar(OrdenAccion.Id);

            _ordenesPorMesa.Remove(idMesa);
            MostrarEliminarOrden = false;
            LimpiarAccionMesa();
            await ShowSuccessMessage(resultado.Finalizada
                ? "La orden tenía pagos y fue finalizada. La mesa quedó libre."
                : "Orden eliminada. La mesa quedó libre.");
        }
        catch (Exception ex)
        {
            await ShowErrorMessage(ex);
        }
        finally
        {
            ProcesandoEliminarOrden = false;
        }
    }

    private void LimpiarAccionMesa()
    {
        MesaAccion = null;
        OrdenAccion = null;
        MesasDisponiblesCambio = [];
        IdMesaDestino = 0;
    }
    #endregion

    #region Utils
    private OrdenMesaDTO? ObtenerOrdenMesa(MesaDTO mesa)
    {
        return _ordenesPorMesa.GetValueOrDefault(mesa.Id);
    }

    private static string ObtenerNumeroOrden(OrdenMesaDTO? ordenMesa)
    {
        if (ordenMesa is null)
        {
            return "la orden";
        }

        if (!string.IsNullOrWhiteSpace(ordenMesa.Numero))
        {
            return ordenMesa.Numero;
        }

        return ordenMesa.Id > 0 ? $"Orden #{ordenMesa.Id}" : "Orden sin guardar";
    }

    private static TimeSpan ObtenerTiempoOrden(OrdenMesaDTO ordenMesa)
    {
        if (ordenMesa.EstadoUsoMesa == (short)EstadoUsoMesa.EnCurso && ordenMesa.FechaInicio != default)
        {
            return DateTime.Now > ordenMesa.FechaInicio
                ? DateTime.Now - ordenMesa.FechaInicio
                : TimeSpan.Zero;
        }

        return TimeSpan.FromMinutes((double)Math.Max(0, ordenMesa.MinutosConsumidos));
    }

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
