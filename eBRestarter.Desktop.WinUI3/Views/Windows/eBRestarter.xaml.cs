using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Desktop.WinUI3.Helpers;
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
    /// Das Hauptfenster der Anwendung.
    /// <br/>
    /// <b>Verantwortlichkeit:</b> Dient als "Shell" (Hülle), die das grundlegende Layout (NavigationView, TitleBar) bereitstellt
    /// und den <see cref="Frame"/> für den Navigationsdienst hostet.
    /// </summary>
    public sealed partial class EBRestarter : Window
    {
        /// <summary>
        /// Der Navigationsdienst, der die Logik für Seitenwechsel kapselt.
        /// </summary>
        private readonly INavigationService _navigationService;

        private readonly IAppVersionInfoService? _iAppVersionInfoService;

        /// <summary>
        /// Die Standard-Animation für Seitenübergünge (hier: "DrillIn" Effekt).
        /// </summary>
        private readonly NavigationTransitionInfo _defaultTransition = new DrillInNavigationTransitionInfo();

        /// <summary>
        /// Initialisiert eine neue Instanz des Hauptfensters.
        /// </summary>
        /// <param name="navigationService">Der injizierte Navigationsdienst (Dependency Injection).</param>
        public EBRestarter(INavigationService navigationService, IAppVersionInfoService iAppVersionInfoService)
        {
            InitializeComponent();

            // 1. SRP (Single Responsibility Principle):
            // Die Konfiguration der Titelleiste (Farben, Verhalten) wurde in eine Extension Method ausgelagert,
            // um den Code-Behind dieser Klasse sauber zu halten.
            this.ConfigureTitleBarColors();

            // Performance-Optimierung: Hült die letzten 10 Seiten im Speicher, um schnelles "Zurück" zu ermüglichen.
            NavigationFrame.CacheSize = 10;

            _navigationService = navigationService;

            _iAppVersionInfoService = iAppVersionInfoService;

            // WICHTIG: Verbindung von UI (View) und Logik (Service).
            // Wir übergeben den XAML-Frame an den Service, damit dieser navigieren kann.
            _navigationService.AttachFrame(NavigationFrame);

            // Initiale Navigation beim Start der App.
            // Wir nutzen den String-Key "CommonOverview", damit das Fenster den konkreten Typ der Page nicht kennen muss.
            _navigationService.NavigateTo("CommonOverview", transitionInfo: new DrillInNavigationTransitionInfo());

            // 1. AppWindow holen (Standard-Prozedur in WinUI 3)
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

            // Fenster- und Taskleisten-Icon setzen (in WinUI 3 zur Laufzeit erforderlich)
            string iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "eB Restarter.ico");

            if (File.Exists(iconPath))
            {
                appWindow.SetIcon(iconPath);
            }

            // 2. Den "Presenter" abrufen und maximieren
            // Der OverlappedPresenter ist der Standard für Desktop-Apps
            if (appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.Maximize();
            }

            TxbVersion.Text = "v" + _iAppVersionInfoService?.RetrieveAppVersion();

            //// 1. Fenster-Handle holen
            //IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

            //// 2. WindowId daraus erstellen
            //Microsoft.UI.WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);

            //// 3. Das AppWindow holen
            //AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

            //// 4. Größe ändern (Breite, Hühe) in Pixeln
            //appWindow.Resize(new SizeInt32(2300, 2080));
        }

        /// <summary>
        /// Event-Handler für den "Zurück"-Button in der benutzerdefinierten Titelleiste.
        /// </summary>
        private void AppTitleBar_BackRequested(TitleBar sender, object args)
        {
            // Prüft direkt am Frame, ob eine Rückwürtsnavigation müglich ist.
            if (NavigationFrame.CanGoBack)
            {
                NavigationFrame.GoBack();
            }
        }

        /// <summary>
        /// Event-Handler für den "Hamburger"-Button (Menü umschalten) in der Titelleiste.
        /// </summary>
        private void AppTitleBar_PaneToggleRequested(TitleBar sender, object args)
        {
            // Klappt das Navigationsmenü auf oder zu.
            NavView.IsPaneOpen = !NavView.IsPaneOpen;
        }

        /// <summary>
        /// Zentraler Handler für Klicks auf Menü-Eintrüge im NavigationView.
        /// <br/>
        /// Vereint die Logik von SelectionChanged und ItemInvoked.
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

