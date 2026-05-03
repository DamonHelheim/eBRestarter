using CommunityToolkit.Mvvm.ComponentModel;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Desktop.WinUI3.Models;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using System.Collections.ObjectModel;

namespace eBRestarter.Desktop.WinUI3.ViewModels;

/// <summary>
/// View model for the About page. Displays the application version from
/// <see cref="IAppInfoService"/> and a list of icon credits, both localized where applicable.
/// </summary>
public partial class ViewModelAbout : ObservableObject
{
    private readonly IAppInfoService _appInfoService;

    private readonly IIconCreditService _iconCreditService;

    private readonly ILocalizationService _localizationService;

    [ObservableProperty] public partial string AppVersion { get; set; }

    /// <summary>Collection of icon credit entries shown on the About page (e.g. author and license).</summary>
    public ObservableCollection<IconCredit> IconCredits { get; } = [];

    /// <summary>
    /// Initializes the About VM with app-info and localization services, sets a loading placeholder
    /// for version, and loads version plus icon credits so the UI can bind immediately.
    /// </summary>
    public ViewModelAbout(
        IAppInfoService appInfoService,
        ILocalizationService localizationService,
        IIconCreditService iconCreditService)
    {
        _appInfoService = appInfoService;
        _localizationService = localizationService;
        _iconCreditService = iconCreditService;
        AppVersion = _localizationService.GetString("About_Loading");
        LoadVersionAndIconCredits();
    }

    /// <summary>Fetches version from app info and icon credits, then updates AppVersion and IconCredits for binding.</summary>
    private void LoadVersionAndIconCredits()
    {
        string prefix = _localizationService.GetString("About_VersionPrefix");
        AppVersion = $"{prefix} {_appInfoService.GetAppVersion()}";
        var credits = _iconCreditService.GetIconCredits();
        IconCredits.Clear();
        foreach (var iconCredit in credits)
            IconCredits.Add(iconCredit);
    }
}
