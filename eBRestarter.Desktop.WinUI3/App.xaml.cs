using eBRestarter.Core.Application.DependencyInjections;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Models;
using eBRestarter.Desktop.WinUI3.DependencyInjections;
using eBRestarter.Desktop.WinUI3.Services;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using eBRestarter.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.Windows.Globalization;
using System;
using System.Globalization;
using System.Threading;


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
             .ConfigureAppConfiguration((_, cfg) =>
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
            // 1. KONFIGURATION (Persistenz) + SPRACHE + THEME (vor Fensterladen)
            // =========================================================================
            StartupDisplayPreferences? launchConfig = null;
            try
            {
                launchConfig = AppHost!.Services.GetRequiredService<IApplicationLaunchConfigService>().PrepareConfigForLaunch();
                string languageCode = launchConfig.LanguageCode;

                ApplicationLanguages.PrimaryLanguageOverride = languageCode;

                CultureInfo culture = new(languageCode);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
            }
            catch (Exception)
            {
                ApplicationLanguages.PrimaryLanguageOverride = "en-US";
            }

            try
            {
                var themeService = AppHost!.Services.GetRequiredService<IThemeService>();
                string themeToSet = launchConfig != null && !string.IsNullOrEmpty(launchConfig.ThemeName)
                    ? launchConfig.ThemeName
                    : "Light";
                themeService.SetTheme(themeToSet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fehler beim Laden des Themes: {ex.Message}");
            }

            // =========================================================================
            // 2. HINTERGRUND-SERVICES STARTEN
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
            // 3. JETZT ERST DAS FENSTER ERSTELLEN (InitializeComponent zieht nun die richtigen Ressourcen)
            // =========================================================================
            MainWindoweBRestarter = AppHost!.Services.GetRequiredService<EBRestarter>();
            AppDispatcherQueue = MainWindoweBRestarter.DispatcherQueue;

            MainWindoweBRestarter.Activate();
        }
    }
}
