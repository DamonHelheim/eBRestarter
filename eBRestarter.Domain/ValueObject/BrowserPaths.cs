using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Domain.ValueObject
{
    public record BrowserPaths(string CacheDir, string CookiesDir, string ExtensionsDir);
}
