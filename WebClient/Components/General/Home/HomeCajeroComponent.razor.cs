using System.Globalization;
using Domain.DTOs.General;

namespace WebClient.Components.General.Home;

public partial class HomeCajeroComponent
{
    private static readonly CultureInfo CurrencyCulture =
        CultureInfo.GetCultureInfo("es-BO");

    private DashboardCajeroDTO? Dashboard { get; set; }
    private bool IsLoading { get; set; } = true;
    private string ErrorMessage { get; set; } = string.Empty;

    private string NombreCajero
    {
        get
        {
            var nombre = Dashboard?.NombreVendedor?.Trim();
            return string.IsNullOrWhiteSpace(nombre)
                ? "Cajero"
                : nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        try
        {
            Dashboard = await AppServices.DashboardService.GetCajero();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static string FormatCurrency(decimal amount)
    {
        return $"Bs {amount.ToString("N2", CurrencyCulture)}";
    }

    private static decimal GetPercentage(decimal amount, decimal total)
    {
        return total <= 0 ? 0 : Math.Min(100, Math.Round(amount / total * 100, 2));
    }

    private static string GetPaymentIcon(DashboardMetodoPagoDTO payment)
    {
        if (!string.IsNullOrWhiteSpace(payment.Icono))
        {
            return payment.Icono;
        }

        if (payment.EsEfectivo)
        {
            return "bi bi-cash";
        }

        var name = payment.Nombre;
        return name.Contains("qr", StringComparison.OrdinalIgnoreCase)
            ? "bi bi-qr-code"
            : "bi bi-credit-card";
    }

    private static string GetPaymentTone(int index)
    {
        string[] tones = ["tone-success", "tone-primary", "tone-warning", "tone-danger"];
        return tones[index % tones.Length];
    }
}
