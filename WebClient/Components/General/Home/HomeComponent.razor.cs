using System.Globalization;
using Domain.DTOs.General;

namespace WebClient.Components.General.Home;

public partial class HomeComponent
{
    private static readonly CultureInfo CurrencyCulture = CultureInfo.GetCultureInfo("es-BO");

    private DashboardDTO? Dashboard { get; set; }
    private bool IsLoading { get; set; } = true;
    private string ErrorMessage { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        try
        {
            Dashboard = await AppServices.DashboardService.Get();
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
        return amount.ToString("C0", CurrencyCulture);
    }
}
