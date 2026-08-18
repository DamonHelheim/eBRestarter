using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;
using Microsoft.Windows.ApplicationModel.Resources;

using eBRestarter.Core.Application.ObjectArchetypes.Constants;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Providers.Interfaces;
using eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.UIOptionDTO;

namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Providers;

/// <summary>
/// Provider implementation for retrieving localized resource strings and UI dropdown options for application settings.
/// </summary>
public sealed class LocalizationProvider : IInboundPortLocalizationProvider, IUIOptionsProvider
{
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

    private readonly ILogger<LocalizationProvider> _logger;

    private readonly ResourceContext _resourceContext;
    private readonly ResourceMap _resourceMap;

    // Caches resolved resource strings in memory to ensure allocation-free O(1) lookups during rapid UI timer ticks.
    private readonly ConcurrentDictionary<string, string> _resourceStringCache = new(StringComparer.Ordinal);

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationProvider"/> class using the current primary language context.
    /// </summary>
    /// <param name="logger">Logger instance for reporting failed localization lookups.</param>
    public LocalizationProvider(ILogger<LocalizationProvider> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;

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

    /// <inheritdoc />
    public string RetrieveString(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return string.Empty;
        }

        // Static lambda using state tuple avoids closure allocations per lookup while logging failed key resolutions.
        return _resourceStringCache.GetOrAdd(
            key,
            static (resourceKey, lookup) =>
            {
                try
                {
                    var result = lookup.Map.GetValue($"{ResourcePathPrefix}{resourceKey}", lookup.Context);

                    if (result?.ValueAsString is { } resolvedValue)
                    {
                        return resolvedValue;
                    }

                    lookup.Logger.LogWarning(
                        LogEventIds.UserInterface.LocalizationLookupFailed,
                        "Resource key {ResourceKey} has no value in the current language; showing the bracketed key instead.",
                        resourceKey);

                    return $"[{resourceKey}]";
                }
                catch (Exception exception)
                {
                    lookup.Logger.LogWarning(
                        LogEventIds.UserInterface.LocalizationLookupFailed,
                        exception,
                        "Looking up resource key {ResourceKey} failed; showing the bracketed key instead.",
                        resourceKey);

                    return $"[{resourceKey}]";
                }
            },
            (Map: _resourceMap, Context: _resourceContext, Logger: _logger));
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public IEnumerable<ComputerRestartOption> GetComputerRestartOptions() =>
    [
        new(RetrieveString(KeyRestartOption0), 0),
        new(RetrieveString(KeyRestartOption1), 1),
        new(RetrieveString(KeyRestartOption3), 3),
        new(RetrieveString(KeyRestartOption7), 7),
        new(RetrieveString(KeyRestartOption14), 14)
    ];

    /// <inheritdoc />
    public IEnumerable<BrowserCacheDeleteOption> GetBrowserCacheOptions() =>
    [
        new(RetrieveString(KeyBrowserCacheOption0), 0),
        new(RetrieveString(KeyBrowserCacheOption1), 1),
        new(RetrieveString(KeyBrowserCacheOption3), 3),
        new(RetrieveString(KeyBrowserCacheOption7), 7),
        new(RetrieveString(KeyBrowserCacheOption14), 14)
    ];
}
