namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

/// <summary>
/// Represents operating system edition and version metadata.
/// </summary>
public sealed record OperatingSystemInfo
{
    /// <summary>
    /// Gets the operating system edition name (e.g. "Windows 11 Pro").
    /// </summary>
    public string Edition { get; init; } = string.Empty;

    /// <summary>
    /// Gets the operating system display version identifier (e.g. "22H2").
    /// </summary>
    public string Version { get; init; } = string.Empty;

    /// <summary>
    /// Gets the operating system build version number (e.g. "22621.123").
    /// </summary>
    public string BuildNumber { get; init; } = string.Empty;
}