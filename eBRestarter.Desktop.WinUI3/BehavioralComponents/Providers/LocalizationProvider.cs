using System.Collections.Generic;

using Microsoft.Windows.ApplicationModel.Resources;

using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Providers.Interfaces;
using eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.UIOptionDTO;

namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Providers;

/// <summary>
/// Provider implementation for retrieving localized resource strings and UI dropdown options for application settings.
/// </summary>
public sealed class LocalizationProvider : IInboundPortLocalizationProvider, IUIOptionsProvider
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    private const char FallbackBracket = '[';
    private const string FallbackDeutsch = "Deutsch";
    private const string FallbackEnglish = "English";

    private const string KeyBrowserCacheOption0 = "BrowserCacheOption_0";
    private const string KeyBrowserCacheOption1 = "BrowserCacheOption_1";
    private const string KeyBrowserCacheOption3 = "BrowserCacheOption_3";
    private const string KeyBrowserCacheOption7 = "BrowserCacheOption_7";
    private const string KeyBrowserCacheOption14 = "BrowserCacheOption_14";

    private const string KeyLangEnglish = "Lang_English";
    private const string KeyLangGerman = "Lang_German";

    private const string KeyQualifierLanguage = "Language";

    private const string KeyRestartOption0 = "RestartOption_0";
    private const string KeyRestartOption1 = "RestartOption_1";
    private const string KeyRestartOption3 = "RestartOption_3";
    private const string KeyRestartOption7 = "RestartOption_7";
    private const string KeyRestartOption14 = "RestartOption_14";

    private const string ResourcePathPrefix = "Resources/";

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    // ── Block 4: Komplexe Typen / Repositories (alphabetisch A–Z) ──
    private readonly ResourceContext _resourceContext;
    private readonly ResourceMap _resourceMap;


    // ═══════════════════════════════════════════════════════
    //  3. Constructors
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationProvider"/> class using the current primary language context.
    /// </summary>
    public LocalizationProvider()
    {
        var resourceManager = new ResourceManager();
        _resourceContext = resourceManager.CreateResourceContext();

        string currentLanguage = Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride;
        if (string.IsNullOrEmpty(currentLanguage))
        {
            currentLanguage = System.Globalization.CultureInfo.CurrentUICulture.Name;
        }

        _resourceContext.QualifierValues[KeyQualifierLanguage] = currentLanguage;
        _resourceMap = resourceManager.MainResourceMap;
    }


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Retrieves a localized string associated with the specified resource key.
    /// </summary>
    /// <param name="key">The resource key identifier.</param>
    /// <returns>The localized string if found; otherwise, a bracketed fallback string containing the key.</returns>
    public string RetrieveString(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return string.Empty;
        }

        try
        {
            var result = _resourceMap.GetValue($"{ResourcePathPrefix}{key}", _resourceContext);
            return result?.ValueAsString ?? $"[{key}]";
        }
        catch
        {
            return $"[{key}]";
        }
    }

    /// <summary>
    /// Retrieves all available language options for UI selection.
    /// </summary>
    /// <returns>A collection of <see cref="LanguageOption"/> objects.</returns>
    public IEnumerable<LanguageOption> GetAvailableLanguages()
    {
        string germanName = RetrieveString(KeyLangGerman);
        string englishName = RetrieveString(KeyLangEnglish);

        if (germanName.StartsWith(FallbackBracket))
        {
            germanName = FallbackDeutsch;
        }

        if (englishName.StartsWith(FallbackBracket))
        {
            englishName = FallbackEnglish;
        }

        return [new(germanName, 0), new(englishName, 1)];
    }

    /// <summary>
    /// Retrieves localized computer restart interval options for UI selection.
    /// </summary>
    /// <returns>A collection of <see cref="ComputerRestartOption"/> objects.</returns>
    public IEnumerable<ComputerRestartOption> GetComputerRestartOptions() =>
    [
        new(RetrieveString(KeyRestartOption0), 0),
        new(RetrieveString(KeyRestartOption1), 1),
        new(RetrieveString(KeyRestartOption3), 3),
        new(RetrieveString(KeyRestartOption7), 7),
        new(RetrieveString(KeyRestartOption14), 14)
    ];

    /// <summary>
    /// Retrieves localized browser cache deletion interval options for UI selection.
    /// </summary>
    /// <returns>A collection of <see cref="BrowserCacheDeleteOption"/> objects.</returns>
    public IEnumerable<BrowserCacheDeleteOption> GetBrowserCacheOptions() =>
    [
        new(RetrieveString(KeyBrowserCacheOption0), 0),
        new(RetrieveString(KeyBrowserCacheOption1), 1),
        new(RetrieveString(KeyBrowserCacheOption3), 3),
        new(RetrieveString(KeyBrowserCacheOption7), 7),
        new(RetrieveString(KeyBrowserCacheOption14), 14)
    ];
}
