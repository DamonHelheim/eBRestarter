using eBRestarter.Core.Domain.Models.Records;

namespace eBRestarter.Core.Application.Interfaces;

public interface ILocalizationService
{
    // Liefert einen einzelnen übersetzten String
    string GetString(string key);

    // Liefert direkt die fertige Liste für die ComboBox
    IEnumerable<LanguageOption> GetAvailableLanguages();

    IEnumerable<ComputerRestartOption> GetComputerRestartOptions();

    IEnumerable<BrowserCacheDeleteOption> GetBrowserCacheOptions();
}
