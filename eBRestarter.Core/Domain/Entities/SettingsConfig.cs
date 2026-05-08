namespace eBRestarter.Core.Domain.Entities;

public class SettingsConfig
{
    public string Theme { get; set; } = "Light";
    public int Language { get; set; }
    public bool StartWithWindows { get; set; }
    public bool StartRestarterWithProgramStart { get; set; }
    public string ApiUsername { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}
