using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using WebClient.Common.Validation;
using WebClient.Components.Inventory.Lote;

namespace WebClient.Components.Inventory.TransaccionInventario;

public partial class TransaccionInventarioCrearComponent
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IValidator<TransaccionInventarioDTO> Validator { get; set; }
    [Parameter] public TransaccionInventarioDTO TransaccionInventario { get; set; }
    [Parameter] public List<AlmacenDTO> ListadoAlmacenes { get; set; } = new();
    [Parameter] public List<ProductoDTO> ListadoProductos { get; set; } = new();
    [Parameter] public List<SelectOptionDTO<short>> ListadoTipos { get; set; } = new();
    public bool IsEditing => TransaccionInventario?.Id > 0;
    public List<TransaccionInventarioDetalleDTO> ListadoDetalles { get; set; } = new();
    public List<LoteDTO> ListadoLotes { get; set; } = new();
    public List<LoteDTO> ListadoLotesDisponibles =>
        ListadoProductos.FirstOrDefault(p => p.Id == DetalleDTO.IdProducto)?.ListadoLotes ?? new List<LoteDTO>();
    private IReadOnlyList<ProductoConversionDTO> ListadoUnidadesDisponibles =>
        ListadoProductos.FirstOrDefault(producto => producto.Id == DetalleDTO.IdProducto)?
            .ProductoConversiones?
            .OrderBy(conversion => conversion.FactorConversion)
            .ThenBy(conversion => conversion.UnidadMedida?.Nombre)
            .ToList() ?? [];
    private TransaccionInventarioDetalleDTO DetalleDTO { get; set; } = new();
    private FluentValidationValidator<TransaccionInventarioDTO> _fvValidator;
    private EditContext? _editContext { get; set; }
    private DotNetObjectReference<TransaccionInventarioCrearComponent>? _objectHelper;
    private LoteCreateComponent ModalCrearLoteComponent;
    public bool MostrarModalLote { get; set; } = false;

    protected override void OnInitialized()
    {
        TransaccionInventario ??= new TransaccionInventarioDTO();
        _editContext = new EditContext(TransaccionInventario);
        _fvValidator = new FluentValidationValidator<TransaccionInventarioDTO>(_editContext, Validator);
    }

    protected override void OnParametersSet()
    {
        TransaccionInventario ??= new TransaccionInventarioDTO();
        ListadoDetalles = TransaccionInventario.Detalles?.ToList() ?? new();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        await InicializarJSHelper();
    }

    private async Task InicializarJSHelper()
    {
        try
        {
            _objectHelper = DotNetObjectReference.Create(this);
            await JSRuntime.InvokeVoidAsync("TransaccionInventarioCrearComponent.init", _objectHelper);
        }
        catch (Exception)
        {
            await JSRuntime.InvokeVoidAsync("console.warn", $"No se pudo inicializar JSHelper para componente: {nameof(TransaccionInventarioCrearComponent)}");
        }
    }
    [JSInvokable]
    public async Task Validar()
    {
        if (_editContext?.Validate() ?? false) await Guardar();
    }

    private async Task Guardar()
    {
        try
        {

            TransaccionInventario.Detalles = ListadoDetalles;
            if (TransaccionInventario.Id != 0)
            {
                var respuesta = await AppServices.TransaccionInventarioService.Update(TransaccionInventario);
            }
            else
            {
                var respuesta = await AppServices.TransaccionInventarioService.Create(TransaccionInventario);
            }

            await ShowSuccessMessage("Movimiento guardado correctamente");
            await Task.Delay(1000);
            Navigation.NavigateTo($"{AdminConfig.General.WebUrl}TransaccionInventario/listado", true);
        }
        catch (Exception ex)
        {
            await ShowErrorMessage(ex.Message);
        }

    }

    public async Task AgregarProducto()
    {
        if (DetalleDTO.IdProducto == 0 ||
            !DetalleDTO.IdProductoConversion.HasValue ||
            DetalleDTO.IdAlmacen == 0 ||
            DetalleDTO.Cantidad <= 0)
        {
            await ShowErrorMessage("Debe seleccionar producto, unidad, almacen y una cantidad mayor a cero.");
            return;
        }

        var detalle = ListadoDetalles.Where(pd => pd.IdProducto == DetalleDTO.IdProducto)
        .Where(pd => pd.IdProductoConversion == DetalleDTO.IdProductoConversion)
        .Where(pd => pd.IdLote == DetalleDTO.IdLote)
        .Where(pd => pd.IdAlmacen == DetalleDTO.IdAlmacen)
        .FirstOrDefault();

        var producto = ListadoProductos.FirstOrDefault(p => p.Id == DetalleDTO.IdProducto);
        var conversion = producto?.ProductoConversiones?
            .FirstOrDefault(item => item.Id == DetalleDTO.IdProductoConversion);
        var almacen = ListadoAlmacenes.FirstOrDefault(p => p.Id == DetalleDTO.IdAlmacen);
        var lote = producto?.ListadoLotes?.FirstOrDefault(l => l.Id == DetalleDTO.IdLote);

        if (conversion is null || conversion.FactorConversion <= 0)
        {
            await ShowErrorMessage("La unidad seleccionada no corresponde al producto.");
            return;
        }

        if (detalle != null)
        {
            detalle.Cantidad = DetalleDTO.Cantidad;
            detalle.NombreProducto = producto?.Nombre ?? "";
            detalle.NombreUnidadMedida = conversion.UnidadMedida?.Nombre ?? "";
            detalle.AbreviaturaUnidadMedida = conversion.UnidadMedida?.Abreviatura ?? "";
            detalle.FactorConversion = conversion.FactorConversion;
            detalle.NombreAlmacen = almacen?.Nombre ?? "";
            detalle.CodigoLote = lote?.Codigo ?? "";
        }
        else
        {
            DetalleDTO.NombreProducto = producto?.Nombre ?? "";
            DetalleDTO.NombreUnidadMedida = conversion.UnidadMedida?.Nombre ?? "";
            DetalleDTO.AbreviaturaUnidadMedida = conversion.UnidadMedida?.Abreviatura ?? "";
            DetalleDTO.FactorConversion = conversion.FactorConversion;
            DetalleDTO.NombreAlmacen = almacen?.Nombre ?? "";
            DetalleDTO.CodigoLote = lote?.Codigo ?? "";
            ListadoDetalles.Add(DetalleDTO);
        }
        DetalleDTO = new();
        StateHasChanged();
    }

    public void Editar(long idProducto, long? idProductoConversion, long? idLote, long idAlmacen)
    {
        var detalle = ListadoDetalles.Where(pd => pd.IdProducto == idProducto)
                         .Where(pd => pd.IdProductoConversion == idProductoConversion)
                         .Where(pd => pd.IdLote == idLote)
                         .Where(pd => pd.IdAlmacen == idAlmacen)
                         .FirstOrDefault();
        if (detalle == null) return;

        DetalleDTO = new TransaccionInventarioDetalleDTO()
        {
            IdAlmacen = detalle.IdAlmacen,
            IdLote = detalle.IdLote,
            IdProducto = detalle.IdProducto,
            IdProductoConversion = detalle.IdProductoConversion,
            Cantidad = detalle.Cantidad,
            NombreProducto = detalle.NombreProducto,
            NombreUnidadMedida = detalle.NombreUnidadMedida,
            AbreviaturaUnidadMedida = detalle.AbreviaturaUnidadMedida,
            FactorConversion = detalle.FactorConversion,
            NombreAlmacen = detalle.NombreAlmacen,
            CodigoLote = detalle.CodigoLote
        };
        StateHasChanged();
    }

    public void EliminarDetalle(long idProducto, long? idProductoConversion, long? idLote, long idAlmacen)
    {
        var detalle = ListadoDetalles.Where(pd => pd.IdProducto == idProducto)
                                 .Where(pd => pd.IdProductoConversion == idProductoConversion)
                                 .Where(pd => pd.IdLote == idLote)
                                 .Where(pd => pd.IdAlmacen == idAlmacen)
                                 .FirstOrDefault();
        if (detalle == null) return;
        ListadoDetalles.Remove(detalle);
        StateHasChanged();
    }

    private void ProductoCambiado(long? idProducto)
    {
        DetalleDTO.IdProducto = idProducto ?? 0;
        DetalleDTO.IdLote = null;

        var conversionPredeterminada = ListadoUnidadesDisponibles
            .FirstOrDefault(conversion => conversion.FactorConversion == 1)
            ?? ListadoUnidadesDisponibles.FirstOrDefault();

        UnidadCambiada(conversionPredeterminada?.Id);
    }

    private void UnidadCambiada(long? idProductoConversion)
    {
        DetalleDTO.IdProductoConversion = idProductoConversion;
        var conversion = ListadoUnidadesDisponibles
            .FirstOrDefault(item => item.Id == idProductoConversion);

        DetalleDTO.NombreUnidadMedida = conversion?.UnidadMedida?.Nombre ?? string.Empty;
        DetalleDTO.AbreviaturaUnidadMedida = conversion?.UnidadMedida?.Abreviatura ?? string.Empty;
        DetalleDTO.FactorConversion = conversion?.FactorConversion ?? 1;
    }

    private static string DescripcionUnidad(ProductoConversionDTO conversion)
    {
        var nombre = conversion.UnidadMedida?.Nombre ?? "Sin unidad";
        return conversion.FactorConversion == 1
            ? $"{nombre} (unidad base)"
            : $"{nombre} (x{conversion.FactorConversion:0.######} base)";
    }

    private static double CantidadBase(TransaccionInventarioDetalleDTO detalle) =>
        detalle.Cantidad * (double)(detalle.FactorConversion > 0 ? detalle.FactorConversion : 1);

    private string UnidadBase(TransaccionInventarioDetalleDTO detalle)
    {
        var unidad = ListadoProductos
            .FirstOrDefault(producto => producto.Id == detalle.IdProducto)?
            .ProductoConversiones?
            .FirstOrDefault(conversion => conversion.FactorConversion == 1)?
            .UnidadMedida;

        return unidad?.Abreviatura ?? unidad?.Nombre ?? "base";
    }

    public void AbrirModalCrearLote()
    {
        MostrarModalLote = true;
        StateHasChanged();
    }

    public void CerrarModalCrearLote()
    {
        MostrarModalLote = false;
        StateHasChanged();
    }

    public async Task LoteCreado(LoteDTO lote)
    {
        ListadoLotes.Add(lote);
        var producto = ListadoProductos.FirstOrDefault(p => p.Id == DetalleDTO.IdProducto);
        producto?.ListadoLotes?.Add(lote);
        DetalleDTO.IdLote = lote.Id;
        CerrarModalCrearLote();
        await ShowSuccessMessage("Lote creado correctamente");
    }

    public async Task GuardarLote()
    {
        await ModalCrearLoteComponent.Validar();
    }

}
