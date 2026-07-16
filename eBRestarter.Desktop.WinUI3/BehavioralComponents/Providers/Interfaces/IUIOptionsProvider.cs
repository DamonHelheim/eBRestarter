using System.Collections.Generic;
using eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.UIOptionDTO;

namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Providers.Interfaces;

public interface IUIOptionsProvider
{
    IEnumerable<LanguageOption> GetAvailableLanguages();
    IEnumerable<ComputerRestartOption> GetComputerRestartOptions();
    IEnumerable<BrowserCacheDeleteOption> GetBrowserCacheOptions();
}
