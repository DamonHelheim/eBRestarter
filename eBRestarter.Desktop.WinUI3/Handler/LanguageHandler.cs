using eBRestarter.Desktop.WinUI3.Handler.Interfaces;
using Microsoft.Windows.Globalization;

namespace eBRestarter.Desktop.WinUI3.Services;

public sealed class LanguageHandler : ILanguageHandler
{
    public string CurrentLanguageCode => ApplicationLanguages.PrimaryLanguageOverride;

    public void SetLanguageOption(string languageCode)
    {
        ApplicationLanguages.PrimaryLanguageOverride = languageCode;
    }
}
