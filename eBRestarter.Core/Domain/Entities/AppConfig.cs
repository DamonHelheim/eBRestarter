namespace eBRestarter.Core.Domain.Entities;

public class AppConfig
{
    public string Username { get; set; } = string.Empty;
    public BrowserConfig Browser { get; set; } = new();
    public SettingsConfig Settings { get; set; } = new();
    public Computer Computer { get; set; } = new();
}
