using System;
using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Application;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Providers.Interfaces;
using eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.PresentationDTO;

namespace eBRestarter.Desktop.WinUI3.ViewModels;

/// <summary>
/// View model for the About page. Displays the application version from
/// <see cref="IOutboundPortAppVersionInfoProvider"/> and a list of icon credits, both localized where applicable.
/// </summary>
public sealed partial class ViewModelAbout : ObservableObject
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    private const string KeyAboutLoading = "About_Loading";
    private const string KeyAboutVersionPrefix = "About_VersionPrefix";

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    // ── Block 1: Injizierte Abhängigkeiten (alphabetisch A–Z) ──
    private readonly IOutboundPortAppVersionInfoProvider _appInfoProvider;
    private readonly IIconCreditProvider _iconCreditService;
    private readonly IInboundPortLocalizationProvider _localizationService;


    // ═══════════════════════════════════════════════════════
    //  3. Constructors
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Initializes the About VM with app-info and localization services, sets a loading placeholder
    /// for version, and loads version plus icon credits so the UI can bind immediately.
    /// </summary>
    public ViewModelAbout(
        IOutboundPortAppVersionInfoProvider appInfoProvider,
        IIconCreditProvider iconCreditService,
        IInboundPortLocalizationProvider localizationService)
    {
        _appInfoProvider = appInfoProvider ?? throw new ArgumentNullException(nameof(appInfoProvider));
        _iconCreditService = iconCreditService ?? throw new ArgumentNullException(nameof(iconCreditService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));

        AppVersion = _localizationService.RetrieveString(KeyAboutLoading);
        LoadVersionAndIconCredits();
    }


    // ═══════════════════════════════════════════════════════
    //  6. Properties
    // ═══════════════════════════════════════════════════════
    /// <summary>Gets or sets the localized application version display string.</summary>
    [ObservableProperty]
    public partial string AppVersion { get; set; }

    /// <summary>Collection of icon credit entries shown on the About page (e.g. author and license).</summary>
    public ObservableCollection<IconCredit> IconCredits { get; } = [];


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════
    /// <summary>Fetches version from app info and icon credits, then updates AppVersion and IconCredits for binding.</summary>
    private void LoadVersionAndIconCredits()
    {
        string prefix = _localizationService.RetrieveString(KeyAboutVersionPrefix);
        AppVersion = $"{prefix} {_appInfoProvider.RetrieveAppVersion()}";

        var credits = _iconCreditService.GetIconCredits();
        IconCredits.Clear();
        foreach (var iconCredit in credits)
        {
            IconCredits.Add(iconCredit);
        }
    }
}
