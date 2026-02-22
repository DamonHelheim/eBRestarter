namespace eBRestarter.Core.Domain.Models.Records;

public record BrowserPaths(
    List<string> CacheDirs,
    List<string> CookiesDirs,
    List<string> ExtensionsDirs
);
