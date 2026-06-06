namespace WebClient.Configs;

public class General
{
    public string Nombre { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string WwwRootPath { get; set; } = string.Empty;
    public string WebUrl { get; set; } = string.Empty;
    public string ApiUrl { get; set; } = string.Empty;
    public int ServiceTimeout { get; set; }
    public int TiempoExpiracionCookie { get; set; }
}
