using eBRestarter.Core.Application.Enums;

namespace eBRestarter.Core.Application.Models;

public sealed class BrowserInfo
{
    public BrowserType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public bool IsInstalled { get; set; }
    public string IconPath { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
}
