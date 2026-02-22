namespace eBRestarter.Core.Application.Interfaces;

/// <summary>
/// Verantwortlichkeit: Vertrag, wie Zeit in Text umgewandelt wird.
/// Layer: Core (Port - Driving Interface)
/// </summary>
public interface ITimeFormatter
{
    string Format(int seconds);
}
