
namespace Domain.Utils;

public static class Utils
{
    public static decimal Redondear(decimal cantidad, short cantidadDecimales = 2)
    {
        return Math.Round(cantidad, cantidadDecimales, MidpointRounding.AwayFromZero);
    }
}
