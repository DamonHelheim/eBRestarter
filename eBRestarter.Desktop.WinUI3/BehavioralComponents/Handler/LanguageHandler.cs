using System;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Handler.Interfaces;
using Microsoft.Windows.Globalization;

namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Handler;

/// <summary>
/// Implementation of <see cref="ILanguageHandler"/> utilizing WinUI ApplicationLanguages API.
/// </summary>
public sealed class LanguageHandler : ILanguageHandler
{
    /// <inheritdoc />
    public string CurrentLanguageCode => ApplicationLanguages.PrimaryLanguageOverride;

    /// <inheritdoc />
    public void SetLanguageOption(string languageCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(languageCode);

        ApplicationLanguages.PrimaryLanguageOverride = languageCode;
    }
}
