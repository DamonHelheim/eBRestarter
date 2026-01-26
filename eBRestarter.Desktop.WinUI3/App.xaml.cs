using eBRestarter.Core.Application.DependencyInjections;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Config; // Namespace für IEVisitorConfigService anpassen
using eBRestarter.Desktop.WinUI3.DependencyInjections;
using eBRestarter.Desktop.WinUI3.Services;
using eBRestarter.Desktop.WinUI3.Services.Interfaces; // Namespace für IThemeService anpassen
using eBRestarter.Desktop.WinUI3.Views; // Namespace für dein MainWindow (EBRestarter)
using eBRestarter.Infrastructure.DependencyInjection;
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
             .ConfigureServices((ctx, services) =>
             {
                 // Deine Service-Registrierungen
                 services.AddInfrastructureServices();
                 services.AddApplicationServices();
                 services.AddNavigationService();
                 services.AddDialoglServiceExtensions();
                 services.AddViewModels();

                 // WICHTIG: Sicherstellen, dass der ThemeService registriert ist
                 // Entweder hier direkt oder in einer deiner Extension-Methoden (z.B. AddApplicationServices)
                 services.AddSingleton<IThemeService, ThemeService>();
                 services.AddSingleton<ILanguageService, LanguageService>();
                 services.AddSingleton<ILocalizationService, LocalizationService>();
             });

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            // 1. Fenster Instanz holen (das erstellt das Fenster, zeigt es aber noch nicht an)
            MainWindoweBRestarter = AppHost!.Services.GetRequiredService<EBRestarter>();

            // 2. Dispatcher Queue speichern
            AppDispatcherQueue = MainWindoweBRestarter.DispatcherQueue;

            // --- SPRACHE INITIALISIEREN ---
            try
            {
                // 1. Config-Service aus dem Dependency Injection Container holen
                // (Benötigt: using Microsoft.Extensions.DependencyInjection;)
                var configService = AppHost.Services.GetRequiredService<IEVisitorConfigService>();

                // 2. Aktuelle Konfiguration laden
                var config = configService.LoadConfig();

                // 3. Sprachcode ermitteln (Mapping: 0 = Deutsch, 1 = Englisch)
                // Sollte mit deiner Logik im ViewModelOptions übereinstimmen
                string languageCode = config.Settings.Language == 0 ? "de-DE" : "en-US";

                // 4. Sprache global für die App setzen
                ApplicationLanguages.PrimaryLanguageOverride = languageCode;
            }
            catch (Exception)
            {
                // Fallback: Falls die Config nicht geladen werden kann (z.B. erster Start oder Fehler),
                // setzen wir einen sicheren Standard (z.B. Englisch oder Systemstandard).
                ApplicationLanguages.PrimaryLanguageOverride = "en-US";
            }

            // =========================================================================
            // THEME INITIALISIERUNG
            // =========================================================================
            try
            {
                // Services aus dem Container holen
                var configService = AppHost.Services.GetRequiredService<IEVisitorConfigService>();
                var themeService = AppHost.Services.GetRequiredService<IThemeService>();

                // Config laden
                var config = configService.LoadConfig();

                // Theme setzen (Fallback auf "Light", falls Config leer ist)
                string themeToSet = string.IsNullOrEmpty(config.Settings.Theme) ? "Light" : config.Settings.Theme;

                themeService.SetTheme(themeToSet);
            }
            catch (Exception ex)
            {
                // Safety-First: Falls beim Theme-Laden was schief geht (z.B. Config korrupt),
                // soll die App trotzdem starten (dann halt im Standard-Theme).
                // Hier könntest du loggen: AppHost.Services.GetRequiredService<ILogger<App>>().LogError(ex, ...);
                System.Diagnostics.Debug.WriteLine($"Fehler beim Laden des Themes: {ex.Message}");
            }
            // =========================================================================

            // 3. Jetzt erst das Fenster anzeigen (jetzt im richtigen Theme)
            MainWindoweBRestarter.Activate();
        }
    }
}