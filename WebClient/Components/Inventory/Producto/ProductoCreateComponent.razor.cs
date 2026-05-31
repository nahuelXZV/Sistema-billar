using Domain.DTOs.Inventory;
using Domain.DTOs.Shared;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using WebClient.Common.Validation;

namespace WebClient.Components.Inventory.Producto;

public partial class ProductoCreateComponent
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IValidator<ProductoDTO> Validator { get; set; }
    [Parameter] public ProductoDTO Producto { get; set; }
    [Parameter] public List<CategoriaDTO> ListadoCategorias { get; set; } = new();
    [Parameter] public List<UnidadMedidaDTO> ListadoUnidadesMedidas { get; set; } = new();
    [Parameter] public List<ProductoDTO> ListadoProductos { get; set; } = new();
    [Parameter] public List<SelectOptionDTO<short>> ListaTiposProductos { get; set; } = new();
    public ProductoCompuestoDTO ProdCompuesto { get; set; } = new();
    public List<ProductoCompuestoDTO> ListadoProdCompuesto { get; set; } = new();
    public bool IsEditing => Producto?.Id > 0;
    public List<ProductoDTO> ListadoProductosDisponibles =>
        ListadoProductos.Where(p => p.Id != Producto?.Id).ToList();
    private FluentValidationValidator<ProductoDTO> _fvValidator;
    private EditContext? _editContext { get; set; }
    private DotNetObjectReference<ProductoCreateComponent>? _objectHelper;

    protected override void OnInitialized()
    {
        Producto ??= new ProductoDTO();
        _editContext = new EditContext(Producto);
        _fvValidator = new FluentValidationValidator<ProductoDTO>(_editContext, Validator);
    }

    protected override void OnParametersSet()
    {
        Producto ??= new ProductoDTO();
        ListadoProdCompuesto = Producto.ProductosCompuestos?.ToList() ?? new();
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
            await JSRuntime.InvokeVoidAsync("ProductoCreateComponent.init", _objectHelper);
        }
        catch (Exception)
        {
            await JSRuntime.InvokeVoidAsync("console.warn", $"No se pudo inicializar JSHelper para componente: {nameof(ProductoCreateComponent)}");
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

            Producto.ProductosCompuestos = ListadoProdCompuesto;
            if (Producto.Id != 0)
            {
                var respuesta = await AppServices.ProductoService.Update(Producto);
            }
            else
            {
                var respuesta = await AppServices.ProductoService.Create(Producto);
            }

            await ShowSuccessMessage("Producto guardado correctamente");
            await Task.Delay(1000);
            Navigation.NavigateTo($"{AdminConfig.General.WebUrl}Producto/listado", true);
        }
        catch (Exception ex)
        {
            await ShowErrorMessage(ex.Message);
        }

    }

    public void AgregarComponente()
    {
        if (ProdCompuesto.IdProductoComponente == 0 || ProdCompuesto.Cantidad <= 0) return;

        var productoComponente = ListadoProductos.FirstOrDefault(p => p.Id == ProdCompuesto.IdProductoComponente);
        if (productoComponente == null) return;

        ProdCompuesto.ProductoComponente = productoComponente;
        ListadoProdCompuesto.Add(ProdCompuesto);
        ProdCompuesto = new ProductoCompuestoDTO();
        StateHasChanged();
    }

    public void EliminarComponente(long id)
    {
        if (id == 0) return;

        var componente = ListadoProdCompuesto.FirstOrDefault(p => p.IdProductoComponente == id);
        if (componente == null) return;

        ListadoProdCompuesto.Remove(componente);
        StateHasChanged();
    }
}
