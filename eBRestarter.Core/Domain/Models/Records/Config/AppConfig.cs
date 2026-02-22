namespace eBRestarter.Core.Domain.Models.Records.Config;

public record AppConfig
{
    public string Username { get; set; } = string.Empty;
    public Browser Browser { get; set; } = new();
    public SettingsConfig Settings { get; set; } = new();
    public Computer Computer { get; set; } = new();
}
