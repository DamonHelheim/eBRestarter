using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using Microsoft.Windows.Globalization;

namespace eBRestarter.Desktop.WinUI3.Services;

public class LanguageService : ILanguageService
{
    public string CurrentLanguageCode => ApplicationLanguages.PrimaryLanguageOverride;

    public void SetLanguage(string languageCode)
    {
        ApplicationLanguages.PrimaryLanguageOverride = languageCode;
    }

}
