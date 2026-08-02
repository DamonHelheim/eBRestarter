using System;

using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;

namespace eBRestarter.Core.Application.BehavioralComponents.Providers;

/// <summary>
/// Provides startup configuration preferences such as language and UI theme derived from the application configuration repository.
/// </summary>
public sealed class StartupConfigProvider(IOutboundPortEVisitorConfigRepository configService) : IInboundPortStartupConfigProvider
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    private const int GermanLanguageCode = 0;

    private const string DefaultEnglishCultureCode = "en-US";
    private const string DefaultGermanCultureCode = "de-DE";
    private const string DefaultThemeName = "Light";

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    private readonly IOutboundPortEVisitorConfigRepository _configService = configService ?? throw new ArgumentNullException(nameof(configService));


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Retrieves the initial startup display preferences including language code and theme name.
    /// </summary>
    /// <returns>A <see cref="StartupDisplayPreferences"/> instance populated with default or configured values.</returns>
    public StartupDisplayPreferences RetrieveStartupPreferences()
    {
        var appConfig = _configService.LoadConfig();

        string languageCode = appConfig?.Settings?.Language == GermanLanguageCode
            ? DefaultGermanCultureCode
            : DefaultEnglishCultureCode;

        string? rawTheme = appConfig?.Settings?.Theme;
        string themeName = string.IsNullOrWhiteSpace(rawTheme)
            ? DefaultThemeName
            : rawTheme;

        return new StartupDisplayPreferences(languageCode, themeName);
    }
}
