namespace eBRestarter.Core.Application.Models.Config;

public record SettingsConfig
{
    public string Theme { get; set; } = "Light";
    public int Language { get; set; }
    public bool StartWithWindows { get; set; } = false;
    public bool  StartRestarterWithProgramStart { get; set; } = false;
    public string ApiUsername { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}
