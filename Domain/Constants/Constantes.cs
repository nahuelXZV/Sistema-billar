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

    public enum EstadoTraspasoInventario
    {
        Confirmado = 1,
        Revertido = 2,
        Anulado = 3
    }

    public enum TipoProducto
    {
        Producto = 1,
        Servicio = 2
    }

    public enum EstadoOrdenVenta
    {
        Abierta = 1,
        Cerrada = 2
    }

    public enum EstadoUsoMesa
    {
        Pendiente = 1,
        EnCurso = 2,
        Finalizado = 3
    }

    public enum EstadoTurnoCaja
    {
        Abierto = 1,
        Cerrado = 2
    }

    public enum EstadoCompra
    {
        Registrada = 1,
        Anulada = 2
    }
}
