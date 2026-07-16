using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Providers.Interfaces;
using eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.UIOptionDTO;
using Microsoft.Windows.ApplicationModel.Resources;
using System.Collections.Generic;

namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Providers;

public sealed class LocalizationProvider : IInboundPortLocalizationProvider, IUIOptionsProvider
{
    private readonly ResourceManager _resourceManager;
    private readonly ResourceContext _resourceContext;
    private readonly ResourceMap _resourceMap;

    public LocalizationProvider()
    {
        _resourceManager = new ResourceManager();
        _resourceContext = _resourceManager.CreateResourceContext();

        string currentLanguage = Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride;

        if (string.IsNullOrEmpty(currentLanguage))
        {
            currentLanguage = System.Globalization.CultureInfo.CurrentUICulture.Name;
        }

        _resourceContext.QualifierValues["Language"] = currentLanguage;
        _resourceMap = _resourceManager.MainResourceMap;
    }

    public string RetrieveString(string key)
    {
        try
        {
            var result = _resourceMap.GetValue($"Resources/{key}", _resourceContext);

            return result?.ValueAsString ?? $"[{key}]";
        }
        catch
        {
            return $"[{key}]";
        }
    }

    public IEnumerable<LanguageOption> GetAvailableLanguages()
    {
        string de = RetrieveString("Lang_German");
        string en = RetrieveString("Lang_English");
        if (de.StartsWith('[')) de = "Deutsch";
        if (en.StartsWith('[')) en = "English";
        return [new(de, 0), new(en, 1)];
    }

    public IEnumerable<ComputerRestartOption> GetComputerRestartOptions()
    {
        return
        [
            new(RetrieveString("RestartOption_0"), 0),
            new(RetrieveString("RestartOption_1"), 1),
            new(RetrieveString("RestartOption_3"), 3),
            new(RetrieveString("RestartOption_7"), 7),
            new(RetrieveString("RestartOption_14"), 14)
        ];
    }

    public IEnumerable<BrowserCacheDeleteOption> GetBrowserCacheOptions()
    {
        return
        [
            new(RetrieveString("BrowserCacheOption_0"), 0),
            new(RetrieveString("BrowserCacheOption_1"), 1),
            new(RetrieveString("BrowserCacheOption_3"), 3),
            new(RetrieveString("BrowserCacheOption_7"), 7),
            new(RetrieveString("BrowserCacheOption_14"), 14)
        ];
    }
}





