namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

/// <summary>
/// Culture and theme resolved from persisted config after launch-time normalization.
/// </summary>
/// <param name="LanguageCode">The normalized ISO culture or language code.</param>
/// <param name="ThemeName">The normalized application theme name.</param>
public sealed record StartupDisplayPreferences(string LanguageCode, string ThemeName);
