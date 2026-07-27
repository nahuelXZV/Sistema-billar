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
    public ProductoConversionDTO NuevaConversion { get; set; } = new() { FactorConversion = 1 };
    public List<ProductoConversionDTO> ListadoConversiones { get; set; } = new();
    public string TabConfiguracionActiva { get; set; } = "compuestos";
    public string? ErrorConversion { get; set; }
    public bool IsEditing => Producto?.Id > 0;
    public List<ProductoDTO> ListadoProductosDisponibles =>
        ListadoProductos.Where(p => p.Id != Producto?.Id).ToList();
    public List<UnidadMedidaDTO> ListadoUnidadesConversionDisponibles =>
        ListadoUnidadesMedidas
            .Where(unidad => ListadoConversiones.All(conversion => conversion.IdUnidadMedida != unidad.Id))
            .ToList();
    private FluentValidationValidator<ProductoDTO> _fvValidator;
    private EditContext? _editContext { get; set; }
    private DotNetObjectReference<ProductoCreateComponent>? _objectHelper;
    private ProductoDTO? _productoInicializado;
    private long _idUnidadBaseSincronizada;

    protected override void OnInitialized()
    {
        Producto ??= new ProductoDTO();
        _editContext = new EditContext(Producto);
        _fvValidator = new FluentValidationValidator<ProductoDTO>(_editContext, Validator);
    }

    protected override void OnParametersSet()
    {
        Producto ??= new ProductoDTO();

        if (ReferenceEquals(_productoInicializado, Producto))
            return;

        _productoInicializado = Producto;
        ListadoProdCompuesto = Producto.ProductosCompuestos?.ToList() ?? new();
        ListadoConversiones = Producto.ProductoConversiones?.ToList() ?? new();
        _idUnidadBaseSincronizada = Producto.IdUnidadMedida;
        AsegurarUnidadBase();
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
            AsegurarUnidadBase();
            Producto.ProductosCompuestos = Producto.EsCompuesto ? ListadoProdCompuesto : [];
            Producto.ProductoConversiones = ListadoConversiones;

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
        if (ListadoProdCompuesto.Any(item => item.IdProductoComponente == ProdCompuesto.IdProductoComponente)) return;

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

    public void CambiarTabConfiguracion(string tab)
    {
        TabConfiguracionActiva = tab;
    }

    public void SincronizarUnidadBase()
    {
        if (Producto.IdUnidadMedida <= 0)
            return;

        if (_idUnidadBaseSincronizada > 0 &&
            _idUnidadBaseSincronizada != Producto.IdUnidadMedida)
        {
            ListadoConversiones.Clear();
            ErrorConversion = "Las conversiones se reiniciaron porque cambió la unidad base.";
        }

        _idUnidadBaseSincronizada = Producto.IdUnidadMedida;
        AsegurarUnidadBase();
    }

    public void AgregarConversion()
    {
        ErrorConversion = null;

        if (NuevaConversion.IdUnidadMedida <= 0)
        {
            ErrorConversion = "Selecciona una unidad de medida.";
            return;
        }

        if (NuevaConversion.FactorConversion <= 0)
        {
            ErrorConversion = "El factor de conversión debe ser mayor a cero.";
            return;
        }

        if (ListadoConversiones.Any(
            conversion => conversion.IdUnidadMedida == NuevaConversion.IdUnidadMedida))
        {
            ErrorConversion = "La unidad de medida ya fue agregada.";
            return;
        }

        var unidadMedida = ListadoUnidadesMedidas.FirstOrDefault(
            unidad => unidad.Id == NuevaConversion.IdUnidadMedida);

        ListadoConversiones.Add(new ProductoConversionDTO
        {
            IdProducto = Producto.Id,
            IdUnidadMedida = NuevaConversion.IdUnidadMedida,
            FactorConversion = NuevaConversion.FactorConversion,
            UnidadMedida = unidadMedida
        });

        NuevaConversion = new ProductoConversionDTO { FactorConversion = 1 };
    }

    public void EliminarConversion(long idUnidadMedida)
    {
        if (idUnidadMedida == Producto.IdUnidadMedida)
            return;

        var conversion = ListadoConversiones.FirstOrDefault(
            item => item.IdUnidadMedida == idUnidadMedida);

        if (conversion == null)
            return;

        ListadoConversiones.Remove(conversion);
        ErrorConversion = null;
    }

    public string ObtenerNombreUnidad(long idUnidadMedida) =>
        ListadoUnidadesMedidas.FirstOrDefault(unidad => unidad.Id == idUnidadMedida)?.Nombre
        ?? "Unidad no encontrada";

    public string ObtenerAbreviaturaUnidad(long idUnidadMedida) =>
        ListadoUnidadesMedidas.FirstOrDefault(unidad => unidad.Id == idUnidadMedida)?.Abreviatura
        ?? string.Empty;

    private void AsegurarUnidadBase()
    {
        if (Producto.IdUnidadMedida <= 0)
            return;

        var unidadBase = ListadoConversiones.FirstOrDefault(
            conversion => conversion.IdUnidadMedida == Producto.IdUnidadMedida);

        if (unidadBase == null)
        {
            ListadoConversiones.Insert(0, new ProductoConversionDTO
            {
                IdProducto = Producto.Id,
                IdUnidadMedida = Producto.IdUnidadMedida,
                FactorConversion = 1,
                UnidadMedida = ListadoUnidadesMedidas.FirstOrDefault(
                    unidad => unidad.Id == Producto.IdUnidadMedida)
            });
        }
        else
        {
            unidadBase.FactorConversion = 1;
        }
    }
}
