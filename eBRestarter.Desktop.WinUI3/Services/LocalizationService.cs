using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Domain.Models.Records;
using Microsoft.Windows.ApplicationModel.Resources; // WICHTIG: Das neue Microsoft-Namespace!
using System.Collections.Generic;

namespace eBRestarter.Desktop.WinUI3.Services;

public class LocalizationService : ILocalizationService
{
    // =========================================================
    // 1. FIELDS & INJECTED SERVICES (Backing state)
    // =========================================================
    #region FieldsAndInjectedServices

    private readonly ResourceManager _resourceManager;
    private readonly ResourceContext _resourceContext;
    private readonly ResourceMap _resourceMap;

    #endregion

    // =========================================================
    // 2. CONSTRUCTOR & FINALIZER (Ctor)
    // =========================================================
    #region ConstructorAndFinalizer

    public LocalizationService()
    {
        // Nutzt den neuen ResourceManager aus dem Windows App SDK
        _resourceManager = new ResourceManager();
        
        // Context erstellen, um Sprache explizit zu setzen
        _resourceContext = _resourceManager.CreateResourceContext();
        
        // Unpackaged Apps ermitteln die Sprache oft nicht automatisch.
        // Wir erzwingen hier die in der App.xaml.cs gesetzte PrimaryLanguageOverride, 
        // oder fallen auf das System-UI zurück.
        string currentLanguage = Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride;
        if (string.IsNullOrEmpty(currentLanguage))
        {
            currentLanguage = System.Globalization.CultureInfo.CurrentUICulture.Name;
        }
        
        // Setze den Language-Qualifier explizit für diesen Context!
        _resourceContext.QualifierValues["Language"] = currentLanguage;

        _resourceMap = _resourceManager.MainResourceMap;
    }

    #endregion

    // =========================================================
    // 3. PUBLIC METHODS
    // =========================================================
    #region PublicMethods

    public string GetString(string key)
    {
        try
        {
            // Sucht den String in der Standard-ResourceMap "Resources" (z.B. Resources.resw)
            var result = _resourceMap.GetValue($"Resources/{key}", _resourceContext);
            return result?.ValueAsString ?? $"[{key}]";
        }
        catch
        {
            // Fallback, falls der Key wirklich nicht existiert
            return $"[{key}]";
        }
    }

    public IEnumerable<LanguageOption> GetAvailableLanguages()
    {
        string de = GetString("Lang_German");
        string en = GetString("Lang_English");
        if (de.StartsWith("[")) de = "Deutsch";
        if (en.StartsWith("[")) en = "English";
        return [new(de, 0), new(en, 1)];
    }

    public IEnumerable<ComputerRestartOption> GetComputerRestartOptions()
    {
        return
        [
            new(GetString("RestartOption_0"), 0),
            new(GetString("RestartOption_1"), 1),
            new(GetString("RestartOption_3"), 3),
            new(GetString("RestartOption_7"), 7),
            new(GetString("RestartOption_14"), 14)
        ];
    }

    public IEnumerable<BrowserCacheDeleteOption> GetBrowserCacheOptions()
    {
        return
        [
            new(GetString("BrowserCacheOption_0"), 0),
            new(GetString("BrowserCacheOption_1"), 1),
            new(GetString("BrowserCacheOption_3"), 3),
            new(GetString("BrowserCacheOption_7"), 7),
            new(GetString("BrowserCacheOption_14"), 14)
        ];
    }

    #endregion
}