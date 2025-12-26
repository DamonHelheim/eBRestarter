using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Domain.Models.Records
{
    public record BrowserPaths(string CacheDir, string CookiesDir, string ExtensionsDir);
}
