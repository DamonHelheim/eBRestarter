namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

public sealed record BrowserPaths(
    List<string> CacheDirs,
    List<string> CookiesDirs,
    List<string> ExtensionsDirs
);
