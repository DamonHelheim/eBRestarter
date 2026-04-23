using System.Collections.Generic;
using eBRestarter.Desktop.WinUI3.Models;

namespace eBRestarter.Desktop.WinUI3.Services.Interfaces;

public interface IUIOptionsService
{
    IEnumerable<LanguageOption> GetAvailableLanguages();
    IEnumerable<ComputerRestartOption> GetComputerRestartOptions();
    IEnumerable<BrowserCacheDeleteOption> GetBrowserCacheOptions();
}
