using eBRestarter.Core.Application.DependencyInjections;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Config; // Namespace für IEVisitorConfigService anpassen
using eBRestarter.Desktop.WinUI3.DependencyInjections;
using eBRestarter.Desktop.WinUI3.Services;
using eBRestarter.Desktop.WinUI3.Services.Interfaces; // Namespace für IThemeService anpassen
using eBRestarter.Infrastructure.DependencyInjection;
using eBRestarter.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.Windows.Globalization;
using System;


namespace eBRestarter.Desktop.WinUI3
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static Window? MainWindoweBRestarter { get; private set; }
        public static IHost? AppHost { get; private set; } // Generic Host
        public static Microsoft.UI.Dispatching.DispatcherQueue? AppDispatcherQueue { get; private set; }

        /// <summary>
        /// Initializes the singleton application object.
        /// </summary>
        public App()
        {
            InitializeComponent();

            AppHost = CreateHostBuilder().Build();
        }

        private static IHostBuilder CreateHostBuilder() => Host.CreateDefaultBuilder()
             .ConfigureAppConfiguration((ctx, cfg) =>
             {
                 cfg.Sources.Clear();
                 cfg.SetBasePath(AppContext.BaseDirectory);
                 cfg.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
             })
             .ConfigureServices((_, services) =>
             {
                 // Deine Service-Registrierungen

                 services.AddInfrastructureServices();
                 services.AddApplicationServices();
                 services.AddNavigationService();
                 services.AddDialoglServiceExtensions();
                 services.AddThemeService();

                 services.AddSingleton<IBrowserExtensionDeploymentService, BrowserExtensionDeploymentService>();

                 services.AddViewModels();

                 services.AddSingleton<ILanguageService, LanguageService>();
                 services.AddSingleton<ILocalizationService, LocalizationService>();
                 services.AddSingleton<IUIOptionsService>(sp => (LocalizationService)sp.GetRequiredService<ILocalizationService>());
                 services.AddSingleton<IIconCreditService, IconCreditService>();
             });

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (await WinUI3XamlPreview.Preview.IsXamlPreviewLaunched())
            {
                return;
            }

            // =========================================================================
            // 1. SPRACHE INITIALISIEREN (MUSS ZWINGEND VOR DEM FENSTER-LADEN PASSIEREN!)
            // =========================================================================
            try
            {
                var configService = AppHost!.Services.GetRequiredService<IEVisitorConfigService>();
                var config = configService.LoadConfig();

                int intervalDays = config.Browser?.DeleteBrowserCacheIntervalDays ?? 0;
                DateTime nextDate = config.Browser?.NextBrowserDeleteCacheDate ?? DateTime.MinValue;

                if (nextDate == DateTime.Today && intervalDays > 0)
                    nextDate = DateTime.Today.AddDays(intervalDays);

                config.Browser?.NextBrowserDeleteCacheDate = nextDate;
                configService.SaveConfig(config);

                // Sprachcode ermitteln
                string languageCode = config.Settings.Language == 0 ? "de-DE" : "en-US";

                // Sprache für XAML und WinUI 3 MRT Core setzen
                ApplicationLanguages.PrimaryLanguageOverride = languageCode;

                // WICHTIG FÜR UNPACKAGED APPS: Fallback-Kontexte für Win32 & MRT Core synchronisieren
                System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo(languageCode);
                System.Threading.Thread.CurrentThread.CurrentCulture = culture;
                System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
                System.Globalization.CultureInfo.DefaultThreadCurrentCulture = culture;
                System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culture;
            }
            catch (Exception)
            {
                ApplicationLanguages.PrimaryLanguageOverride = "en-US";
            }

            // =========================================================================
            // 2. THEME INITIALISIERUNG (AUCH VOR DEM FENSTER MACHEN)
            // =========================================================================
            try
            {
                var configService = AppHost!.Services.GetRequiredService<IEVisitorConfigService>();
                var themeService = AppHost.Services.GetRequiredService<IThemeService>();
                var config = configService.LoadConfig();

                string themeToSet = string.IsNullOrEmpty(config.Settings.Theme) ? "Light" : config.Settings.Theme;
                themeService.SetTheme(themeToSet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fehler beim Laden des Themes: {ex.Message}");
            }

            // =========================================================================
            // 3. HINTERGRUND-SERVICES STARTEN
            // =========================================================================
            try
            {
                var restartScheduler = AppHost!.Services.GetRequiredService<IComputerRestartScheduler>();
                restartScheduler.StartScheduler();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fehler beim Starten des ComputerRestartSchedulers: {ex.Message}");
            }

            // =========================================================================
            // 4. JETZT ERST DAS FENSTER ERSTELLEN (InitializeComponent zieht nun die richtigen Ressourcen)
            // =========================================================================
            MainWindoweBRestarter = AppHost!.Services.GetRequiredService<EBRestarter>();
            AppDispatcherQueue = MainWindoweBRestarter.DispatcherQueue;

            MainWindoweBRestarter.Activate();
        }
    }
}
