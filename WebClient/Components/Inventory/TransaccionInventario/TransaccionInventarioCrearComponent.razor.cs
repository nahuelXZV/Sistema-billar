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
    public List<TransaccionInventarioDetalleDTO> ListadoDetalles { get; set; } = new();
    public List<LoteDTO> ListadoLotes { get; set; } = new();
    private TransaccionInventarioDetalleDTO DetalleDTO { get; set; } = new();
    private FluentValidationValidator<TransaccionInventarioDTO> _fvValidator;
    private EditContext? _editContext { get; set; }
    private DotNetObjectReference<TransaccionInventarioCrearComponent>? _objectHelper;
    private LoteCreateComponent ModalCrearLoteComponent;
    public bool MostrarModalLote { get; set; } = false;

    protected override void OnInitialized()
    {
        _editContext = new EditContext(TransaccionInventario);
        _fvValidator = new FluentValidationValidator<TransaccionInventarioDTO>(_editContext, Validator);
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
        if (DetalleDTO.IdProducto == 0)
        {
            await ShowErrorMessage("Debe seleccionar un producto y asignar una cantidad mayor a cero.");
            return;
        }
        var detalle = ListadoDetalles.Where(pd => pd.IdProducto == DetalleDTO.IdProducto)
        .Where(pd => pd.IdLote == DetalleDTO.IdLote)
        .Where(pd => pd.IdAlmacen == DetalleDTO.IdAlmacen)
        .FirstOrDefault();

        var producto = ListadoProductos.FirstOrDefault(p => p.Id == DetalleDTO.IdProducto);
        var almacen = ListadoAlmacenes.FirstOrDefault(p => p.Id == DetalleDTO.IdAlmacen);
        var lote = producto?.ListadoLotes?.FirstOrDefault(l => l.Id == DetalleDTO.IdLote);

        if (detalle != null)
        {
            detalle.Cantidad = DetalleDTO.Cantidad;
            detalle.NombreProducto = producto?.Nombre ?? "";
            detalle.NombreAlmacen = almacen?.Nombre ?? "";
            detalle.CodigoLote = lote?.Codigo ?? "";
        }
        else
        {
            DetalleDTO.NombreProducto = producto?.Nombre ?? "";
            DetalleDTO.NombreAlmacen = almacen?.Nombre ?? "";
            DetalleDTO.CodigoLote = lote?.Codigo ?? "";
            ListadoDetalles.Add(DetalleDTO);
        }
        DetalleDTO = new();
        StateHasChanged();
    }

    public void Editar(long idProducto, long? idLote, long idAlmacen)
    {
        var detalle = ListadoDetalles.Where(pd => pd.IdProducto == idProducto)
                         .Where(pd => pd.IdLote == idLote)
                         .Where(pd => pd.IdAlmacen == idAlmacen)
                         .FirstOrDefault();
        if (detalle == null) return;

        DetalleDTO = new TransaccionInventarioDetalleDTO()
        {
            IdAlmacen = detalle.IdAlmacen,
            IdLote = detalle.IdLote,
            IdProducto = detalle.IdProducto,
            Cantidad = detalle.Cantidad,
        };
        StateHasChanged();
    }

    public void EliminarDetalle(long idProducto, long? idLote, long idAlmacen)
    {
        var detalle = ListadoDetalles.Where(pd => pd.IdProducto == idProducto)
                                 .Where(pd => pd.IdLote == idLote)
                                 .Where(pd => pd.IdAlmacen == idAlmacen)
                                 .FirstOrDefault();
        if (detalle == null) return;
        ListadoDetalles.Remove(detalle);
        StateHasChanged();
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
        DetalleDTO.IdLote = lote.Id;
        CerrarModalCrearLote();
        await ShowSuccessMessage("Lote creado correctamente");
    }

    public async Task GuardarLote()
    {
        await ModalCrearLoteComponent.Validar();
    }

}
