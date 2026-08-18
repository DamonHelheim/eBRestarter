namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Handler.Interfaces;

/// <summary>
/// Handler for querying and overriding the primary application language setting.
/// </summary>
public interface ILanguageHandler
{
    /// <summary>
    /// Gets the primary language override code currently configured for the application.
    /// </summary>
    string CurrentLanguageCode { get; }

    /// <summary>
    /// Sets the primary application language override code.
    /// </summary>
    /// <param name="languageCode">The BCP-47 language tag (e.g. "en-US", "de-DE").</param>
    void SetLanguageOption(string languageCode);
}
