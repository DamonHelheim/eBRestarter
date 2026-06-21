using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using Microsoft.Windows.Globalization;

namespace eBRestarter.Desktop.WinUI3.Services;

public class LanguageHandler : ILanguageHandler
{
    public string CurrentLanguageCode => ApplicationLanguages.PrimaryLanguageOverride;

    public void SetLanguage(string languageCode)
    {
        ApplicationLanguages.PrimaryLanguageOverride = languageCode;
    }
}
