namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

/// <summary>
/// Encapsulates hardware specification metadata for diagnostic and display purposes.
/// </summary>
public sealed record HardwareInfo
{
    /// <summary>
    /// Gets the name of the system processor unit.
    /// </summary>
    public string ProcessorName { get; init; } = "Unknown";

    /// <summary>
    /// Gets the name of the primary graphics adapter.
    /// </summary>
    public string GraphicsCardName { get; init; } = "Unknown";

    /// <summary>
    /// Gets the total capacity of installed system memory formatted as text.
    /// </summary>
    public string InstalledRam { get; init; } = "0 bytes";
}