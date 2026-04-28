using eBRestarter.Core.Application.Enums;

namespace eBRestarter.Core.Application.Interfaces.Browser;

/// <summary>
/// Port: Anzeigename des Browsers (lokalisiert) in BrowserType umwandeln.
/// </summary>
public interface IBrowserDisplayNameResolver
{
    // =========================================================
    // 1. PUBLIC METHODS (API / Vertrag)
    // =========================================================
    #region PublicMethods

    /// <summary>
    /// Ermittelt den BrowserType aus dem Anzeigenamen (z.B. ComboBox-Text).
    /// </summary>
    /// <param name="displayName">Anzeigename oder leer/null</param>
    /// <param name="defaultDisplayText">Lokalisierten Text fÃ¼r "Nicht gewÃ¤hlt" (zum Abgleich)</param>
    /// <returns>BrowserType, bei unbekannt/leer Fallback auf Chrome</returns>
    BrowserType GetBrowserTypeFromDisplayName(string displayName, string defaultDisplayText);

    #endregion
}
