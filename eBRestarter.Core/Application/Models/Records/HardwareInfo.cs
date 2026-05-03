namespace eBRestarter.Core.Application.Models.Records;

// Record für Hardware-Daten
public sealed record HardwareInfo
{
    public string ProcessorName { get; init; } = "Unbekannt";
    public string GraphicsCardName { get; init; } = "Unbekannt";
    public string InstalledRam { get; init; } = "0 bytes";
}
