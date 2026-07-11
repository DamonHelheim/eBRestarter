namespace eBRestarter.Core.Domain.ValueObjects;

public sealed record SettingsConfig
{
    public string Theme { get; set; } = "Light";
    public int Language { get; set; }
    public bool StartWithWindows { get; set; }
    public bool StartRestarterWithProgramStart { get; set; }
    public string ApiUsername { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}
