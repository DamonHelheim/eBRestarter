namespace eBRestarter.Core.Application.Models.Records;

public sealed record BrowserPaths(
    List<string> CacheDirs,
    List<string> CookiesDirs,
    List<string> ExtensionsDirs
);
