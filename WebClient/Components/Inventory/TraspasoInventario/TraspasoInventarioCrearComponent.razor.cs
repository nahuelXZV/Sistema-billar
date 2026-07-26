using Domain.DTOs.Inventory;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using WebClient.Common.Validation;

namespace WebClient.Components.Inventory.TraspasoInventario;

public partial class TraspasoInventarioCrearComponent
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IValidator<TraspasoInventarioDTO> Validator { get; set; }
    [Parameter] public TraspasoInventarioDTO Traspaso { get; set; } = new();
    [Parameter] public List<AlmacenDTO> ListadoAlmacenes { get; set; } = new();

    public List<InventarioDTO> InventariosDisponibles { get; set; } = new();
    public List<TraspasoInventarioDetalleDTO> ListadoDetalles { get; set; } = new();
    public List<AlmacenDTO> ListadoAlmacenesDestino => ListadoAlmacenes
        .Where(almacen => almacen.Id != Traspaso.IdAlmacenOrigen)
        .ToList();

    private long IdInventarioSeleccionado { get; set; }
    private decimal Cantidad { get; set; }
    private EditContext? _editContext { get; set; }
    private FluentValidationValidator<TraspasoInventarioDTO> _fvValidator;
    private DotNetObjectReference<TraspasoInventarioCrearComponent>? _objectHelper;

    protected override void OnInitialized()
    {
        _editContext = new EditContext(Traspaso);
        _fvValidator = new FluentValidationValidator<TraspasoInventarioDTO>(_editContext, Validator);
        ListadoDetalles = Traspaso.Detalles.ToList();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        try
        {
            _objectHelper = DotNetObjectReference.Create(this);
            await JSRuntime.InvokeVoidAsync("TraspasoInventarioCrearComponent.init", _objectHelper);
        }
        catch (Exception)
        {
            await JSRuntime.InvokeVoidAsync("console.warn", $"No se pudo inicializar JSHelper para componente: {nameof(TraspasoInventarioCrearComponent)}");
        }
    }

    [JSInvokable]
    public async Task Validar()
    {
        Traspaso.Detalles = ListadoDetalles;
        if (_editContext?.Validate() ?? false) await Guardar();
    }

    private async Task Guardar()
    {
        try
        {
            await AppServices.TraspasoInventarioService.Create(Traspaso);
            await ShowSuccessMessage("Traspaso registrado correctamente.");
            await Task.Delay(1000);
            Navigation.NavigateTo($"{AdminConfig.General.WebUrl}TraspasoInventario/Listado", true);
        }
        catch (Exception ex)
        {
            await ShowErrorMessage(ex.Message);
        }
    }

    private async Task SeleccionarAlmacenOrigen(long idAlmacen)
    {
        Traspaso.IdAlmacenOrigen = idAlmacen;
        if (Traspaso.IdAlmacenDestino == idAlmacen) Traspaso.IdAlmacenDestino = 0;

        ListadoDetalles.Clear();
        IdInventarioSeleccionado = 0;
        Cantidad = 0;

        if (idAlmacen == 0)
        {
            InventariosDisponibles = new();
            return;
        }

        try
        {
            InventariosDisponibles = await AppServices.TraspasoInventarioService.GetInventariosDisponibles(idAlmacen);
        }
        catch (Exception ex)
        {
            InventariosDisponibles = new();
            await ShowErrorMessage(ex.Message);
        }
    }

    private void CambiarFecha(DateTime? fecha)
    {
        Traspaso.Fecha = fecha ?? default;
    }

    private async Task AgregarDetalle()
    {
        var inventario = InventariosDisponibles.FirstOrDefault(item => item.Id == IdInventarioSeleccionado);
        if (inventario == null || Cantidad <= 0)
        {
            await ShowErrorMessage("Debe seleccionar un producto disponible e indicar una cantidad mayor a cero.");
            return;
        }

        var disponible = Convert.ToDecimal(inventario.Cantidad - inventario.Reservado);
        if (Cantidad > disponible)
        {
            await ShowErrorMessage($"La cantidad solicitada supera el stock disponible ({disponible}).");
            return;
        }

        var detalle = ListadoDetalles.FirstOrDefault(item =>
            item.IdProducto == inventario.IdProducto && item.IdLote == inventario.IdLote);

        if (detalle == null)
        {
            ListadoDetalles.Add(new TraspasoInventarioDetalleDTO
            {
                IdProducto = inventario.IdProducto,
                IdLote = inventario.IdLote,
                Cantidad = Cantidad,
                NombreProducto = inventario.Producto?.Nombre ?? string.Empty,
                CodigoLote = inventario.Lote?.Codigo ?? string.Empty
            });
        }
        else
        {
            detalle.Cantidad = Cantidad;
        }

        IdInventarioSeleccionado = 0;
        Cantidad = 0;
    }

    private void EliminarDetalle(TraspasoInventarioDetalleDTO detalle)
    {
        ListadoDetalles.Remove(detalle);
    }

    private string DescripcionInventario(InventarioDTO inventario)
    {
        var lote = string.IsNullOrWhiteSpace(inventario.Lote?.Codigo) ? "Sin lote" : $"Lote: {inventario.Lote.Codigo}";
        var disponible = inventario.Cantidad - inventario.Reservado;
        return $"{inventario.Producto?.Nombre} - {lote} (Disponible: {disponible})";
    }
}
