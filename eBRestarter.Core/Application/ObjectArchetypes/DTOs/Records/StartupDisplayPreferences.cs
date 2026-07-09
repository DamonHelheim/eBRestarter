namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

/// <summary>
/// Culture and theme resolved from persisted config after launch-time normalization.
/// </summary>
public sealed record StartupDisplayPreferences(string LanguageCode, string ThemeName);
