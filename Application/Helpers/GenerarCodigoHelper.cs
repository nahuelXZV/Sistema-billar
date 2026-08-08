namespace Application.Helpers;

public static class GenerarCodigoHelper
{
    public static string Generar(string prefijo, long identificador, DateTime? fecha = null)
    {
        var serial = $"{prefijo.ToUpper().Trim()}-";

        if (fecha != null && fecha != DateTime.MinValue)
            serial += $"{fecha:yyyyMMdd}-";

        serial += $"{identificador:D8}";

        return serial;
    }
}
