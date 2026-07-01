using CommunityToolkit.Mvvm.ComponentModel;
using eBRestarter.Core.Application.Ports.Outbound.Application;
using eBRestarter.Core.Application.Ports.Inbound.Providers;
using eBRestarter.Desktop.WinUI3.Models;
using eBRestarter.Desktop.WinUI3.Providers.Interfaces;
using System.Collections.ObjectModel;

namespace eBRestarter.Desktop.WinUI3.ViewModels;

/// <summary>
/// View model for the About page. Displays the application version from
/// <see cref="IAppInfoProviderOutboundPort"/> and a list of icon credits, both localized where applicable.
/// </summary>
public sealed partial class ViewModelAbout : ObservableObject
{
    private readonly IAppInfoProviderOutboundPort _appInfoProvider;

    private readonly IIconCreditProvider _iconCreditService;

    private readonly ILocalizationProvider _localizationService;

    [ObservableProperty] public partial string AppVersion { get; set; }

    /// <summary>Collection of icon credit entries shown on the About page (e.g. author and license).</summary>
    public ObservableCollection<IconCredit> IconCredits { get; } = [];

    /// <summary>
    /// Initializes the About VM with app-info and localization services, sets a loading placeholder
    /// for version, and loads version plus icon credits so the UI can bind immediately.
    /// </summary>
    public ViewModelAbout(
        IAppInfoProviderOutboundPort appInfoProvider,
        ILocalizationProvider LocalizationProvider,
        IIconCreditProvider iconCreditService)
    {
        _appInfoProvider = appInfoProvider;
        _localizationService = LocalizationProvider;
        _iconCreditService = iconCreditService;
        AppVersion = _localizationService.RetrieveString("About_Loading");
        LoadVersionAndIconCredits();
    }

    /// <summary>Fetches version from app info and icon credits, then updates AppVersion and IconCredits for binding.</summary>
    private void LoadVersionAndIconCredits()
    {
        string prefix = _localizationService.RetrieveString("About_VersionPrefix");
        AppVersion = $"{prefix} {_appInfoProvider.RetrieveAppVersion()}";
        var credits = _iconCreditService.GetIconCredits();
        IconCredits.Clear();
        foreach (var iconCredit in credits)
            IconCredits.Add(iconCredit);
    }
}






