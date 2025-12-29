using eBRestarter.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Application.Interfaces.Browser
{
    public interface IBrowserService
    {
        Task<IEnumerable<BrowserInfo>> GetInstalledBrowsersAsync();
        // Eventuell Methoden für Installation, Download etc.
    }
}
