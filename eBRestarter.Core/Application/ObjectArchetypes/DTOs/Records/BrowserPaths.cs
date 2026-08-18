namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

/// <summary>
/// Encapsulates file system directory paths relevant for browser data cleanup operations.
/// </summary>
/// <param name="CacheDirs">The collection of browser cache directory paths.</param>
/// <param name="CookiesDirs">The collection of browser cookie directory paths.</param>
/// <param name="ExtensionsDirs">The collection of browser extension directory paths.</param>
public sealed record BrowserPaths(
    List<string> CacheDirs,
    List<string> CookiesDirs,
    List<string> ExtensionsDirs
);
