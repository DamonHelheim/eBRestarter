using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Core.Application.Ports.Outbound.Application;
using eBRestarter.Desktop.WinUI3.Helpers.Interfaces;
using eBRestarter.Desktop.WinUI3.Providers.Interfaces;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.IO;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace eBRestarter.Desktop.WinUI3
{
    /// <summary>
    /// The main window of the application.
    /// <br/>
    /// <b>Responsibility:</b> Serves as the "Shell" wrapper providing the basic layout (NavigationView, TitleBar)
    /// and hosting the <see cref="Frame"/> utilized by the navigation service.
    /// </summary>
    public sealed partial class EBRestarter : Window
    {
        /// <summary>
        /// The navigation service encapsulating the page transition logic.
        /// </summary>
        private readonly INavigationService _navigationService;

        private readonly IAppVersionInfoProviderOutboundPort? _iAppVersionInfoService;
        private readonly IMainWindowProvider _mainWindowProvider;
        private readonly IAppWindowHelper _appWindowHelper;

        /// <summary>
        /// The default animation for page transitions (here: "DrillIn" effect).
        /// </summary>
        private readonly NavigationTransitionInfo _defaultTransition = new DrillInNavigationTransitionInfo();

        /// <summary>
        /// Initializes a new instance of the main window.
        /// </summary>
        /// <param name="navigationService">The injected navigation service (Dependency Injection).</param>
        public EBRestarter(
            INavigationService navigationService,
            IAppVersionInfoProviderOutboundPort iAppVersionInfoService,
            IMainWindowProvider mainWindowProvider,
            IAppWindowHelper appWindowHelper)
        {
            InitializeComponent();

            _mainWindowProvider = mainWindowProvider;
            _appWindowHelper = appWindowHelper;

            _mainWindowProvider.SetMainWindow(this);

            // 1. SRP (Single Responsibility Principle):
            // The configuration of the title bar (colors, behavior) has been moved to a separate service
            // to keep this class's code-behind clean.
            _appWindowHelper.ConfigureTitleBarColors(this);

            // Performance optimization: Caches the last 10 pages in memory to enable fast back-navigation.
            NavigationFrame.CacheSize = 10;

            _navigationService = navigationService;

            _iAppVersionInfoService = iAppVersionInfoService;

            // IMPORTANT: Connecting UI (View) and logic (Service).
            // We pass the XAML frame to the service so it can handle navigation.
            _navigationService.AttachFrame(NavigationFrame);

            // Initial navigation on application startup.
            // We use the string key "CommonOverview" so the window doesn't need to know the concrete type of the Page.
            _navigationService.NavigateTo("CommonOverview", transitionInfo: new DrillInNavigationTransitionInfo());

            // 1. Retrieve AppWindow (Standard procedure in WinUI 3)
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

            // Set window and taskbar icon (required at runtime in WinUI 3)
            string iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "eB Restarter.ico");

            if (File.Exists(iconPath))
            {
                appWindow.SetIcon(iconPath);
            }

            // 2. Retrieve the "Presenter" and maximize
            // OverlappedPresenter is the default for desktop applications
            if (appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.Maximize();
            }

            TxbVersion.Text = "v" + _iAppVersionInfoService?.RetrieveAppVersion();

            //// 1. Get window handle
            //IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

            //// 2. Create WindowId from it
            //Microsoft.UI.WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);

            //// 3. Get the AppWindow
            //AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

            //// 4. Resize (Width, Height) in pixels
            //appWindow.Resize(new SizeInt32(2300, 2080));
        }

        /// <summary>
        /// Event handler for the "Back" button in the custom title bar.
        /// </summary>
        private void AppTitleBar_BackRequested(TitleBar sender, object args)
        {
            // Checks directly against the frame if backward navigation is possible.
            if (NavigationFrame.CanGoBack)
            {
                NavigationFrame.GoBack();
            }
        }

        /// <summary>
        /// Event handler for the "Hamburger" button (menu toggle) in the title bar.
        /// </summary>
        private void AppTitleBar_PaneToggleRequested(TitleBar sender, object args)
        {
            // Opens or closes the navigation menu pane.
            NavView.IsPaneOpen = !NavView.IsPaneOpen;
        }

        /// <summary>
        /// Central handler for clicks on menu items within the NavigationView.
        /// <br/>
        /// Unifies the logic for SelectionChanged and ItemInvoked.
        /// </summary>
        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            ArgumentNullException.ThrowIfNull(sender);
            ArgumentNullException.ThrowIfNull(args);

            if (args.InvokedItemContainer is NavigationViewItem nvi
                && nvi.Tag is string tag)
            {
                _navigationService.NavigateTo(
                    tag,
                    parameter: null!,
                    transitionInfo: _defaultTransition
                );
            }
        }
    }
}




