using Domain.DTOs.Contact;
using Domain.DTOs.Inventory;
using Domain.DTOs.Purchases;
using Domain.Validators.Purchases;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using WebClient.Common.Validation;

namespace WebClient.Components.Purchases.Compra;

public partial class CompraCrearComponent
{
    [Inject] public required NavigationManager Navigation { get; set; }
    [Inject] public required IValidator<CompraDTO> Validator { get; set; }

    [Parameter] public required CompraDTO Compra { get; set; }
    [Parameter] public List<ProveedorDTO> ListaProveedores { get; set; } = [];
    [Parameter] public List<AlmacenDTO> ListaAlmacenes { get; set; } = [];
    [Parameter] public List<ProductoDTO> ListaProductos { get; set; } = [];

    private EditContext? _editContext;
    private FluentValidationValidator<CompraDTO>? _fvValidator;
    private DotNetObjectReference<CompraCrearComponent>? _objectHelper;
    private long IdProductoSeleccionado { get; set; }
    private long? IdConversionSeleccionada { get; set; }
    private long? IdLoteSeleccionado { get; set; }
    private decimal Cantidad { get; set; }
    private decimal CostoUnitario { get; set; }
    private decimal Descuento { get; set; }
    private List<CompraDetalleDTO> ListadoDetalles { get; set; } = [];
    private decimal TotalCompra => ListadoDetalles.Sum(ObtenerTotalDetalle);

    private List<ProductoConversionDTO> ConversionesDisponibles => ListaProductos
        .FirstOrDefault(producto => producto.Id == IdProductoSeleccionado)?
        .ProductoConversiones?
        .OrderBy(conversion => conversion.FactorConversion)
        .ToList() ?? [];

    private List<LoteDTO> LotesDisponibles => ListaProductos
        .FirstOrDefault(producto => producto.Id == IdProductoSeleccionado)?
        .ListadoLotes?
        .Where(lote => lote.Activo)
        .OrderBy(lote => lote.Codigo)
        .ToList() ?? [];

    protected override void OnInitialized()
    {
        Compra.IdempotencyKey ??= Guid.NewGuid();
        ListadoDetalles = Compra.ListaDetalles.ToList();
        _editContext = new EditContext(Compra);
        _fvValidator = new FluentValidationValidator<CompraDTO>(_editContext, Validator);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        try
        {
            _objectHelper = DotNetObjectReference.Create(this);
            await JSRuntime.InvokeVoidAsync("CompraCrearComponent.init", _objectHelper);
        }
        catch
        {
            await JSRuntime.InvokeVoidAsync("console.warn", $"No se pudo inicializar JSHelper para componente: {nameof(CompraCrearComponent)}");
        }
    }

    [JSInvokable]
    public async Task Validar()
    {
        Compra.ListaDetalles = ListadoDetalles.ToList();

        if (_editContext?.Validate() ?? false)
        {
            await Guardar();
        }
    }

    private async Task Guardar()
    {
        try
        {
            await AppServices.CompraService.Create(Compra);
            await ShowSuccessMessage("Compra registrada correctamente.");
            await Task.Delay(1000);
            Navigation.NavigateTo($"{AdminConfig.General.WebUrl}Compra/Listado", true);
        }
        catch (Exception ex)
        {
            await ShowErrorMessage(ex.Message);
        }
    }

    private void CambiarFecha(DateTime? fecha) => Compra.Fecha = fecha ?? default;

    private void ProveedorSeleccionado()
    {
        if (IdProductoSeleccionado > 0)
        {
            ActualizarCostoReferencial();
        }
    }

    private void ProductoSeleccionado()
    {
        IdConversionSeleccionada = ConversionesDisponibles.FirstOrDefault()?.Id;
        IdLoteSeleccionado = null;
        ActualizarCostoReferencial();
    }

    private void ConversionSeleccionada() => ActualizarCostoReferencial();

    private async Task AgregarDetalle()
    {
        if (IdProductoSeleccionado <= 0 || !IdConversionSeleccionada.HasValue || IdConversionSeleccionada <= 0 || Cantidad <= 0 || CostoUnitario <= 0)
        {
            await ShowErrorMessage("Debe seleccionar producto, unidad de medida, cantidad y un costo mayor a cero.");
            return;
        }

        var producto = ListaProductos.FirstOrDefault(item => item.Id == IdProductoSeleccionado);
        var conversion = ConversionesDisponibles.FirstOrDefault(item => item.Id == IdConversionSeleccionada);
        if (producto == null || conversion == null)
        {
            await ShowErrorMessage("La presentación seleccionada no corresponde al producto.");
            return;
        }

        var detalleDuplicado = ListadoDetalles.Any(item =>
            item.IdProducto == IdProductoSeleccionado &&
            item.IdProductoConversion == IdConversionSeleccionada &&
            item.IdLote == IdLoteSeleccionado);

        if (detalleDuplicado)
        {
            await ShowErrorMessage("Este producto, presentación y lote ya fueron agregados.");
            return;
        }

        var subtotal = Cantidad * CostoUnitario;
        if (Descuento > subtotal)
        {
            await ShowErrorMessage("El descuento no puede superar el subtotal del detalle.");
            return;
        }

        ListadoDetalles.Add(new CompraDetalleDTO
        {
            IdProducto = producto.Id,
            IdProductoConversion = conversion.Id,
            IdLote = IdLoteSeleccionado,
            Cantidad = Cantidad,
            CostoUnitario = CostoUnitario,
            Descuento = Descuento
        });

        LimpiarDetalle();
    }

    private void EliminarDetalle(CompraDetalleDTO detalle) => ListadoDetalles.Remove(detalle);

    private void ActualizarCostoReferencial()
    {
        var costo = ListaProveedores
            .FirstOrDefault(proveedor => proveedor.Id == Compra.IdProveedor)?
            .ListaProductos
            .FirstOrDefault(item =>
                item.IdProducto == IdProductoSeleccionado &&
                item.IdProductoConversion == IdConversionSeleccionada);

        CostoUnitario = costo?.CostoReferencial ?? 0;
    }

    private void LimpiarDetalle()
    {
        IdProductoSeleccionado = 0;
        IdConversionSeleccionada = null;
        IdLoteSeleccionado = null;
        Cantidad = 0;
        CostoUnitario = 0;
        Descuento = 0;
    }

    private string ObtenerNombreProducto(CompraDetalleDTO detalle) =>
        ListaProductos.FirstOrDefault(producto => producto.Id == detalle.IdProducto)?.Nombre ?? "Producto no encontrado";

    private string ObtenerNombreUnidad(CompraDetalleDTO detalle) =>
        ListaProductos.FirstOrDefault(producto => producto.Id == detalle.IdProducto)?
            .ProductoConversiones?
            .FirstOrDefault(conversion => conversion.Id == detalle.IdProductoConversion)?
            .UnidadMedida?.Nombre ?? "Unidad no encontrada";

    private string ObtenerCodigoLote(CompraDetalleDTO detalle) =>
        detalle.IdLote.HasValue
            ? ListaProductos.FirstOrDefault(producto => producto.Id == detalle.IdProducto)?
                .ListadoLotes?
                .FirstOrDefault(lote => lote.Id == detalle.IdLote)?.Codigo ?? "-"
            : "Sin lote";

    private static decimal ObtenerTotalDetalle(CompraDetalleDTO detalle) =>
        decimal.Round((detalle.Cantidad * detalle.CostoUnitario) - detalle.Descuento, 2);

    private static string ObtenerEtiquetaConversion(ProductoConversionDTO conversion)
    {
        var unidad = conversion.UnidadMedida?.Nombre ?? "Unidad";
        return conversion.FactorConversion == 1 ? $"{unidad} (base)" : $"{unidad} (x{conversion.FactorConversion})";
    }
}
