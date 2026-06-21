using CommunityToolkit.Mvvm.ComponentModel;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Desktop.WinUI3.Models;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using System.Collections.ObjectModel;

namespace eBRestarter.Desktop.WinUI3.ViewModels;

/// <summary>
/// View model for the About page. Displays the application version from
/// <see cref="IAppInfoUseCase"/> and a list of icon credits, both localized where applicable.
/// </summary>
public partial class ViewModelAbout : ObservableObject
{
    private readonly IAppInfoUseCase _appInfoUseCase;

    private readonly IIconCreditHandler _iconCreditService;

    private readonly ILocalizationService _localizationService;

    [ObservableProperty] public partial string AppVersion { get; set; }

    /// <summary>Collection of icon credit entries shown on the About page (e.g. author and license).</summary>
    public ObservableCollection<IconCredit> IconCredits { get; } = [];

    /// <summary>
    /// Initializes the About VM with app-info and localization services, sets a loading placeholder
    /// for version, and loads version plus icon credits so the UI can bind immediately.
    /// </summary>
    public ViewModelAbout(
        IAppInfoUseCase AppInfoAdapter,
        ILocalizationService localizationService,
        IIconCreditHandler iconCreditService)
    {
        _appInfoUseCase = AppInfoAdapter;
        _localizationService = localizationService;
        _iconCreditService = iconCreditService;
        AppVersion = _localizationService.RetrieveString("About_Loading");
        LoadVersionAndIconCredits();
    }

    /// <summary>Fetches version from app info and icon credits, then updates AppVersion and IconCredits for binding.</summary>
    private void LoadVersionAndIconCredits()
    {
        string prefix = _localizationService.RetrieveString("About_VersionPrefix");
        AppVersion = $"{prefix} {_appInfoUseCase.RetrieveAppVersion()}";
        var credits = _iconCreditService.GetIconCredits();
        IconCredits.Clear();
        foreach (var iconCredit in credits)
            IconCredits.Add(iconCredit);
    }
}

