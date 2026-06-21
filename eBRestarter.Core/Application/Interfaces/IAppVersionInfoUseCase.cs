namespace eBRestarter.Core.Application.Interfaces;

/// <summary>
/// Stellt grundlegende Informationen über die Anwendung bereit.
/// </summary>
public interface IAppVersionInfoUseCase
{
    /// <summary>
    /// Gibt die aktuelle Versionsnummer der Anwendung zurück (z.B. "1.0.2.0").
    /// </summary>
    string RetrieveAppVersion();
}
