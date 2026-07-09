namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

public sealed record OperatingSystemInfo
{
    public string Edition { get; init; } = string.Empty;       // e.g., Windows 11 Pro (from WMI)
    public string Version { get; init; } = string.Empty;       // e.g., 22H2 (from Registry)
    public string BuildNumber { get; init; } = string.Empty;   // e.g., 22621.123 (from Registry)
}