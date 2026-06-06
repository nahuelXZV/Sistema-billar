using Domain.DTOs.Sales;
using Microsoft.AspNetCore.Components;

namespace WebClient.Components.Sales.Venta;

public partial class VentaDetalleComponent
{
    [Parameter, EditorRequired]
    public VentaDTO Venta { get; set; } = new();

    private string Estado => Venta.Estado switch
    {
        0 => "Registrada",
        1 => "Pagada",
        2 => "Anulada",
        _ => $"Estado {Venta.Estado}"
    };

    private string TonoEstado => Venta.Estado switch
    {
        1 => "tone-success",
        2 => "tone-danger",
        _ => "tone-primary"
    };
}
