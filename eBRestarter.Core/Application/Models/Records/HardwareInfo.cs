namespace eBRestarter.Core.Application.Models.Records;

public sealed record HardwareInfo
{
    public string ProcessorName { get; init; } = "Unknown";
    public string GraphicsCardName { get; init; } = "Unknown";
    public string InstalledRam { get; init; } = "0 bytes";
}