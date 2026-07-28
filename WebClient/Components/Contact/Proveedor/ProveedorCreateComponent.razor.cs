using Domain.DTOs.Contact;
using Domain.DTOs.Inventory;
using Domain.DTOs.Purchases;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using WebClient.Common.Validation;

namespace WebClient.Components.Contact.Proveedor;

public partial class ProveedorCreateComponent
{
    [Inject] public required NavigationManager Navigation { get; set; }
    [Inject] public required IValidator<ProveedorDTO> Validator { get; set; }

    [Parameter] public required ProveedorDTO Proveedor { get; set; }
    [Parameter] public List<ProductoDTO> ListaProductos { get; set; } = [];

    private EditContext? _editContext;
    private FluentValidationValidator<ProveedorDTO>? _fvValidator;
    private DotNetObjectReference<ProveedorCreateComponent>? _objectHelper;
    private ProveedorDTO? _proveedorInicializado;
    private ProveedorProductoDTO CostoFormulario { get; set; } = new();
    private ProveedorProductoDTO? _costoEnEdicion;
    private List<ProveedorProductoDTO> ListaCostos { get; set; } = [];
    private long IdProductoSeleccionado { get; set; }
    private bool IsEditing => Proveedor.Id > 0;

    private List<ProductoDTO> ProductosDisponibles => ListaProductos
        .Where(producto => producto.Activo)
        .OrderBy(producto => producto.Nombre)
        .ToList();

    private List<ProductoConversionDTO> ConversionesProductoSeleccionado => ListaProductos
        .FirstOrDefault(producto => producto.Id == IdProductoSeleccionado)?
        .ProductoConversiones?
        .Where(conversion => conversion.Id > 0)
        .OrderBy(conversion => conversion.FactorConversion)
        .ToList() ?? [];

    protected override void OnInitialized()
    {
        Proveedor ??= new ProveedorDTO { Activo = true, ListaProductos = [] };
        _editContext = new EditContext(Proveedor);
        _fvValidator = new FluentValidationValidator<ProveedorDTO>(_editContext, Validator);
    }

    protected override void OnParametersSet()
    {
        Proveedor ??= new ProveedorDTO { Activo = true, ListaProductos = [] };

        if (ReferenceEquals(_proveedorInicializado, Proveedor))
        {
            return;
        }

        _proveedorInicializado = Proveedor;
        ListaCostos = Proveedor.ListaProductos?.Select(costo => new ProveedorProductoDTO
        {
            Id = costo.Id,
            IdProveedor = costo.IdProveedor,
            IdProducto = costo.IdProducto,
            IdProductoConversion = costo.IdProductoConversion,
            CostoReferencial = costo.CostoReferencial,
            FechaActualizacion = costo.FechaActualizacion
        }).ToList() ?? [];
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        try
        {
            _objectHelper = DotNetObjectReference.Create(this);
            await JSRuntime.InvokeVoidAsync("ProveedorCreateComponent.init", _objectHelper);
        }
        catch
        {
            await JSRuntime.InvokeVoidAsync("console.warn", $"No se pudo inicializar JSHelper para componente: {nameof(ProveedorCreateComponent)}");
        }
    }

    [JSInvokable]
    public async Task Validar()
    {
        PrepararCostosParaGuardar();

        if (_editContext?.Validate() ?? false)
        {
            await Guardar();
        }
    }

    private async Task Guardar()
    {
        try
        {
            if (Proveedor.Id > 0)
            {
                await AppServices.ProveedorService.Update(Proveedor);
            }
            else
            {
                await AppServices.ProveedorService.Create(Proveedor);
            }

            await ShowSuccessMessage("Proveedor guardado correctamente");
            await Task.Delay(1000);
            Navigation.NavigateTo($"{AdminConfig.General.WebUrl}Proveedor/listado", true);
        }
        catch (Exception ex)
        {
            await ShowErrorMessage(ex.Message);
        }
    }

    private async Task AgregarCosto()
    {
        if (IdProductoSeleccionado <= 0 ||
            !CostoFormulario.IdProductoConversion.HasValue ||
            CostoFormulario.IdProductoConversion <= 0 ||
            CostoFormulario.CostoReferencial <= 0)
        {
            await ShowErrorMessage("Debe seleccionar un producto, una unidad de medida y un costo mayor a cero.");
            return;
        }

        var conversion = ConversionesProductoSeleccionado.FirstOrDefault(
            item => item.Id == CostoFormulario.IdProductoConversion.Value);

        if (conversion == null)
        {
            await ShowErrorMessage("La unidad de medida no pertenece al producto seleccionado.");
            return;
        }

        var costoDuplicado = ListaCostos.FirstOrDefault(costo =>
            costo.IdProducto == IdProductoSeleccionado &&
            costo.IdProductoConversion == conversion.Id);

        if (_costoEnEdicion != null)
        {
            if (costoDuplicado != null && !ReferenceEquals(costoDuplicado, _costoEnEdicion))
            {
                await ShowErrorMessage("El producto ya tiene un costo para esta presentación.");
                return;
            }

            _costoEnEdicion.IdProducto = IdProductoSeleccionado;
            _costoEnEdicion.IdProductoConversion = conversion.Id;
            _costoEnEdicion.CostoReferencial = CostoFormulario.CostoReferencial;
        }
        else if (costoDuplicado != null)
        {
            costoDuplicado.CostoReferencial = CostoFormulario.CostoReferencial;
        }
        else
        {
            ListaCostos.Add(new ProveedorProductoDTO
            {
                IdProveedor = Proveedor.Id,
                IdProducto = IdProductoSeleccionado,
                IdProductoConversion = conversion.Id,
                CostoReferencial = CostoFormulario.CostoReferencial
            });
        }

        LimpiarFormularioCosto();
        StateHasChanged();
    }

    private void EditarCosto(ProveedorProductoDTO costo)
    {
        IdProductoSeleccionado = costo.IdProducto;
        _costoEnEdicion = costo;
        CostoFormulario = new ProveedorProductoDTO
        {
            Id = costo.Id,
            IdProducto = costo.IdProducto,
            IdProductoConversion = costo.IdProductoConversion,
            CostoReferencial = costo.CostoReferencial
        };
    }

    private void EliminarCosto(ProveedorProductoDTO costo)
    {
        ListaCostos.Remove(costo);

        if (ReferenceEquals(_costoEnEdicion, costo))
        {
            LimpiarFormularioCosto();
        }
    }

    private void ProductoSeleccionado()
    {
        CostoFormulario.IdProductoConversion = null;
    }

    private void LimpiarFormularioCosto()
    {
        IdProductoSeleccionado = 0;
        CostoFormulario = new ProveedorProductoDTO();
        _costoEnEdicion = null;
    }

    private void PrepararCostosParaGuardar()
    {
        Proveedor.ListaProductos = ListaCostos.Select(costo => new ProveedorProductoDTO
        {
            Id = costo.Id,
            IdProveedor = Proveedor.Id,
            IdProducto = costo.IdProducto,
            IdProductoConversion = costo.IdProductoConversion,
            CostoReferencial = costo.CostoReferencial,
            FechaActualizacion = costo.FechaActualizacion
        }).ToList();
    }

    private string ObtenerNombreProducto(ProveedorProductoDTO costo) =>
        ListaProductos.FirstOrDefault(producto => producto.Id == costo.IdProducto)?.Nombre
        ?? "Producto no encontrado";

    private string ObtenerNombreUnidad(ProveedorProductoDTO costo)
    {
        var conversion = ObtenerConversion(costo);
        return conversion?.UnidadMedida?.Nombre ?? "Unidad no encontrada";
    }

    private decimal ObtenerFactorConversion(ProveedorProductoDTO costo) =>
        ObtenerConversion(costo)?.FactorConversion ?? 1;

    private ProductoConversionDTO? ObtenerConversion(ProveedorProductoDTO costo) =>
        ListaProductos
            .FirstOrDefault(producto => producto.Id == costo.IdProducto)?
            .ProductoConversiones?
            .FirstOrDefault(conversion => conversion.Id == costo.IdProductoConversion);

    private static string ObtenerEtiquetaConversion(ProductoConversionDTO conversion)
    {
        var unidad = conversion.UnidadMedida?.Nombre ?? "Unidad";
        return conversion.FactorConversion == 1
            ? $"{unidad} (base)"
            : $"{unidad} (x{conversion.FactorConversion})";
    }
}
