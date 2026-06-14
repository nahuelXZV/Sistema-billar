using System.Globalization;

namespace WebClient.Extensions;

public static class UtilsExtensions
{
    public static string FormatearCantidad(this decimal cantidad)
    {
        return cantidad.ToString("0.##", CultureInfo.InvariantCulture);
    }

    public static void Redondear(this decimal cantidad, short cantidadDecimales = 2)
    {
        cantidad = Math.Round(cantidad, cantidadDecimales, MidpointRounding.AwayFromZero);
    }

    public static string FormatoDinero(this decimal cantidad, short cantidadDecimales = 2)
    {
        return $"Bs {cantidad:N2}";
    }

    public static string FormatoTiempo(this TimeSpan tiempo)
    {
        return $"{(int)tiempo.TotalHours:00}:{tiempo.Minutes:00}:{tiempo.Seconds:00}";
    }
}
