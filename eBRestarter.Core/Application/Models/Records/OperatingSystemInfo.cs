namespace eBRestarter.Core.Application.Models.Records;

// Record für OS-Daten (Erweitert das, was dein Registry-Service schon liefert)
public record OperatingSystemInfo
{
    public string Edition { get; init; } = string.Empty;       // z.B. Windows 11 Pro (aus WMI)
    public string Version { get; init; } = string.Empty;       // z.B. 22H2 (aus Registry)
    public string BuildNumber { get; init; } = string.Empty;   // z.B. 22621.123 (aus Registry)
}
