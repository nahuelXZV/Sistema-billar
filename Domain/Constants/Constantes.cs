namespace Domain.Constants;

public static class Constantes
{
    public static class HttpClientNames
    {
        public const string ApiRest = "ApiRest";
    }

    public static class CorsPolicies
    {
        public const string ClienteWeb = "ClienteWeb";
        public const string AllowOrigin = "AllowOrigin";
    }

    public enum TipoUnidadMedida
    {
        Unidad = 1,
        Peso = 2,
        Volumen = 3,
        Longitud = 4,
        Tiempo = 5
    }

    public enum TipoTransaccionInventario
    {
        Ingreso = 1,
        Salida = 2,
        Merma = 3,
    }

    public enum TipoProducto
    {
        Producto = 1,
        Servicio = 2
    }
}
