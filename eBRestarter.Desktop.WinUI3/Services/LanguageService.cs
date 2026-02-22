using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using Microsoft.Windows.Globalization;

namespace eBRestarter.Desktop.WinUI3.Services;

public class LanguageService : ILanguageService
{
    // =========================================================
    // 1. PUBLIC PROPERTIES (Data & State)
    // =========================================================
    #region PublicProperties

    public string CurrentLanguageCode => ApplicationLanguages.PrimaryLanguageOverride;

    #endregion

    // =========================================================
    // 2. PUBLIC METHODS
    // =========================================================
    #region PublicMethods

    public void SetLanguage(string languageCode)
    {
        ApplicationLanguages.PrimaryLanguageOverride = languageCode;
    }

    #endregion
}
