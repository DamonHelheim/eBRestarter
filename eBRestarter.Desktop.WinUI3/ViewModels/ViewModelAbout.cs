using CommunityToolkit.Mvvm.ComponentModel;
using eBRestarter.Desktop.WinUI3.Providers.Interfaces;
using System.Collections.ObjectModel;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Application;
using eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.PresentationDTO;

namespace eBRestarter.Desktop.WinUI3.ViewModels;

/// <summary>
/// View model for the About page. Displays the application version from
/// <see cref="IOutboundPortAppVersionInfoProvider"/> and a list of icon credits, both localized where applicable.
/// </summary>
public sealed partial class ViewModelAbout : ObservableObject
{
    private readonly IOutboundPortAppVersionInfoProvider _appInfoProvider;

    private readonly IIconCreditProvider _iconCreditService;

    private readonly IInboundPortLocalizationProvider _localizationService;

    [ObservableProperty] public partial string AppVersion { get; set; }

    /// <summary>Collection of icon credit entries shown on the About page (e.g. author and license).</summary>
    public ObservableCollection<IconCredit> IconCredits { get; } = [];

    /// <summary>
    /// Initializes the About VM with app-info and localization services, sets a loading placeholder
    /// for version, and loads version plus icon credits so the UI can bind immediately.
    /// </summary>
    public ViewModelAbout(
        IOutboundPortAppVersionInfoProvider appInfoProvider,
        IInboundPortLocalizationProvider LocalizationProvider,
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






