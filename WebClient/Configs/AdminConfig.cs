namespace WebClient.Configs;

public class AdminConfig
{
    public General General { get; set; } = new();
    public Personalizaciones Personalizaciones { get; set; } = new();
}
