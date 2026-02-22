namespace eBRestarter.Desktop.WinUI3.Services.Interfaces;

public interface ILanguageService
{
    // =========================================================
    // 1. PUBLIC PROPERTIES (Contract)
    // =========================================================
    #region PublicProperties

    string CurrentLanguageCode { get; }

    #endregion

    // =========================================================
    // 2. PUBLIC METHODS (API / Contract)
    // =========================================================
    #region PublicMethods

    void SetLanguage(string languageCode);

    #endregion
}
