namespace eBRestarter.Core.Application.Ports.Outbound.Formatters;

/// <summary>
/// Responsibility: Contract defining how time is formatted into text.
/// Layer: Core (Port - Driving Interface)
/// </summary>
public interface ITimeFormatter
{
    string Format(int seconds);
}