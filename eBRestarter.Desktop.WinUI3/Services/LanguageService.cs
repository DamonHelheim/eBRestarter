using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using Microsoft.Windows.Globalization;

namespace eBRestarter.Desktop.WinUI3.Services;

public sealed class LanguageService : ILanguageService
{
    public string CurrentLanguageCode => ApplicationLanguages.PrimaryLanguageOverride;

    public void SetLanguageOption(string languageCode)
    {
        ApplicationLanguages.PrimaryLanguageOverride = languageCode;
    }
}
