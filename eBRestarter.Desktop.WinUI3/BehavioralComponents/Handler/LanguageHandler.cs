using System;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Handler.Interfaces;
using Microsoft.Windows.Globalization;

namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Handler;

public sealed class LanguageHandler : ILanguageHandler
{
    public string CurrentLanguageCode => ApplicationLanguages.PrimaryLanguageOverride;

    public void SetLanguageOption(string languageCode)
    {
        // ✅ .NET 10 Guard Clause
        ArgumentException.ThrowIfNullOrWhiteSpace(languageCode);

        ApplicationLanguages.PrimaryLanguageOverride = languageCode;
    }
}
