using System.Collections.Generic;
using eBRestarter.Desktop.WinUI3.Models;
using eBRestarter.Desktop.WinUI3.ObjectArchetypes.UIOptionDTO;

namespace eBRestarter.Desktop.WinUI3.Providers.Interfaces;

public interface IUIOptionsProvider
{
    IEnumerable<LanguageOption> GetAvailableLanguages();
    IEnumerable<ComputerRestartOption> GetComputerRestartOptions();
    IEnumerable<BrowserCacheDeleteOption> GetBrowserCacheOptions();
}
