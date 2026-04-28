namespace eBRestarter.Core.Application.Models.Records;

public record BrowserPaths(
    List<string> CacheDirs,
    List<string> CookiesDirs,
    List<string> ExtensionsDirs
);
