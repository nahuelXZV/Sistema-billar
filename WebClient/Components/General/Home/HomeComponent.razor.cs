using System.Globalization;
using ApexCharts;
using Domain.DTOs.General;

namespace WebClient.Components.General.Home;

public partial class HomeComponent
{
    private static readonly CultureInfo CurrencyCulture = CultureInfo.GetCultureInfo("es-BO");
    private static readonly IReadOnlyList<KeyValuePair<int, string>> Months =
        Enumerable.Range(1, 12)
            .Select(month => new KeyValuePair<int, string>(
                month,
                CultureInfo.GetCultureInfo("es-BO").TextInfo.ToTitleCase(
                    new DateTime(2000, month, 1).ToString("MMMM", CultureInfo.GetCultureInfo("es-BO")))))
            .ToList();

    private static readonly IReadOnlyList<int> Years =
        Enumerable.Range(DateTime.Today.Year - 5, 6).Reverse().ToList();

    private static readonly ApexChartOptions<DashboardComparisonItemDTO> ComparisonChartOptions = new()
    {
        Colors = ["#31ae7f", "#b86a3f"],
        DataLabels = new DataLabels { Enabled = false }
    };

    private static readonly ApexChartOptions<DashboardChartItemDTO> ResultChartOptions = new()
    {
        Colors = ["#b86a3f", "#31ae7f"],
        DataLabels = new DataLabels { Enabled = false }
    };

    private DashboardDTO? Dashboard { get; set; }
    private bool IsLoading { get; set; } = true;
    private string ErrorMessage { get; set; } = string.Empty;
    private int SelectedMonth { get; set; } = DateTime.Today.Month;
    private int SelectedYear { get; set; } = DateTime.Today.Year;

    private List<DashboardChartItemDTO> MonthlyResult => Dashboard is null
        ? []
        :
        [
            new DashboardChartItemDTO
            {
                Etiqueta = "Costo de ventas",
                Valor = Math.Max(Dashboard.CostoVentas, 0)
            },
            new DashboardChartItemDTO
            {
                Etiqueta = "Utilidad",
                Valor = Math.Max(Dashboard.UtilidadBruta, 0)
            }
        ];

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadDashboardAsync();
    }

    private async Task LoadDashboardAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            Dashboard = await AppServices.DashboardService.Get(SelectedMonth, SelectedYear);
            SelectedMonth = Dashboard.Mes;
            SelectedYear = Dashboard.Anio;
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

    private static string FormatPercentage(decimal amount)
    {
        return $"{amount.ToString("N1", CurrencyCulture)}%";
    }

    private static decimal GetShare(decimal value, decimal total)
    {
        return total <= 0 ? 0 : Math.Round(value / total * 100, 1);
    }

    private static string GetBarWidth(
        decimal value,
        IReadOnlyCollection<DashboardChartItemDTO> items)
    {
        var maximum = items.Count == 0 ? 0 : items.Max(item => item.Valor);
        if (maximum <= 0 || value <= 0)
        {
            return "0";
        }

        var percentage = Math.Max(value / maximum * 100, 6);
        return percentage.ToString("0.##", CultureInfo.InvariantCulture);
    }
}
