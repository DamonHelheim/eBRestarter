using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.IO;

using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Application;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Providers.Interfaces;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Services.Interfaces;
using eBRestarter.Desktop.WinUI3.BehavioralUIComponents.Helpers.Interfaces;

namespace eBRestarter.Desktop.WinUI3;

/// <summary>
/// The main window of the application.
/// <br/>
/// <b>Responsibility:</b> Serves as the "Shell" wrapper providing the basic layout (NavigationView, TitleBar)
/// and hosting the <see cref="Frame"/> utilized by the navigation service.
/// </summary>
public sealed partial class EBRestarter : Window
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitives & strings ──
    private const string ApplicationIconFileName = "eB Restarter.ico";
    private const string AssetsFolderName = "Assets";
    private const string CommonOverviewPageTag = "CommonOverview";
    private const int NavigationFrameCacheSize = 10;

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    // ── Block 1: Injected dependencies ──
    private readonly IAppWindowHelper _appWindowHelper;
    private readonly IOutboundPortAppVersionInfoProvider? _appVersionInfoService;
    private readonly IMainWindowProvider _mainWindowProvider;
    private readonly INavigationService _navigationService;

    // ── Block 4: Complex types, collections & UI elements ──
    /// <summary>
    /// The default animation for page transitions (here: "DrillIn" effect).
    /// </summary>
    private readonly NavigationTransitionInfo _defaultTransition = new DrillInNavigationTransitionInfo();

    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Initializes a new instance of the main window.
    /// </summary>
    /// <param name="appWindowHelper">Helper for window title bar styling and bounds configuration.</param>
    /// <param name="appVersionInfoService">Provider for application version metadata.</param>
    /// <param name="mainWindowProvider">Service for storing global main window reference.</param>
    /// <param name="navigationService">Service for navigating between application pages.</param>
    public EBRestarter(
        IAppWindowHelper appWindowHelper,
        IOutboundPortAppVersionInfoProvider appVersionInfoService,
        IMainWindowProvider mainWindowProvider,
        INavigationService navigationService)
    {
        ArgumentNullException.ThrowIfNull(appWindowHelper);
        ArgumentNullException.ThrowIfNull(appVersionInfoService);
        ArgumentNullException.ThrowIfNull(mainWindowProvider);
        ArgumentNullException.ThrowIfNull(navigationService);

        InitializeComponent();

        _appWindowHelper = appWindowHelper;
        _appVersionInfoService = appVersionInfoService;
        _mainWindowProvider = mainWindowProvider;
        _navigationService = navigationService;

        _mainWindowProvider.SetMainWindow(this);

        _appWindowHelper.ConfigureTitleBarColors(this);

        NavigationFrame.CacheSize = NavigationFrameCacheSize;

        _navigationService.AttachFrame(NavigationFrame);

        _navigationService.NavigateTo(CommonOverviewPageTag, transitionInfo: new DrillInNavigationTransitionInfo());

        IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
        AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

        string iconPath = Path.Combine(AppContext.BaseDirectory, AssetsFolderName, ApplicationIconFileName);

        if (File.Exists(iconPath))
        {
            appWindow.SetIcon(iconPath);
        }

        if (appWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.Maximize();
        }

        TxbVersion.Text = $"v{_appVersionInfoService?.RetrieveAppVersion()}";
    }

    // ═══════════════════════════════════════════════════════
    //  8. Methods (public → private)
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Event handler for the "Back" button in the custom title bar.
    /// </summary>
    /// <param name="sender">The title bar instance.</param>
    /// <param name="eventArgs">Event arguments.</param>
    private void AppTitleBar_BackRequested(TitleBar sender, object eventArgs)
    {
        if (!NavigationFrame.CanGoBack)
        {
            return;
        }

        NavigationFrame.GoBack();
    }

    /// <summary>
    /// Event handler for the "Hamburger" button (menu toggle) in the title bar.
    /// </summary>
    /// <param name="sender">The title bar instance.</param>
    /// <param name="eventArgs">Event arguments.</param>
    private void AppTitleBar_PaneToggleRequested(TitleBar sender, object eventArgs)
    {
        NavView.IsPaneOpen = !NavView.IsPaneOpen;
    }

    /// <summary>
    /// Central handler for clicks on menu items within the NavigationView.
    /// Unifies the logic for SelectionChanged and ItemInvoked.
    /// </summary>
    /// <param name="sender">The navigation view instance.</param>
    /// <param name="eventArgs">Arguments describing the invoked item and container.</param>
    private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs eventArgs)
    {
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentNullException.ThrowIfNull(eventArgs);

        if (eventArgs.InvokedItemContainer is not NavigationViewItem navigationViewItem ||
            navigationViewItem.Tag is not string pageTag)
        {
            return;
        }

        _navigationService.NavigateTo(
            pageTag,
            parameter: null!,
            transitionInfo: _defaultTransition);
    }
}
