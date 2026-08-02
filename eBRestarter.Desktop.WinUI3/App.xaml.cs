using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.Windows.Globalization;
using System;
using System.Globalization;
using System.Threading;

using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Services;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Extensions.DependencyInjections;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Handler;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Handler.Interfaces;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Providers;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Providers.Interfaces;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Utilities;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Utilities.Interfaces;
using eBRestarter.Desktop.WinUI3.BehavioralUIComponents.Helpers;
using eBRestarter.Desktop.WinUI3.BehavioralUIComponents.Helpers.Interfaces;
using eBRestarter.Desktop.WinUI3.Extensions.DependencyInjections;
using eBRestarter.Infrastructure.BehavioralComponents.Extensions.DependencyInjection;

namespace eBRestarter.Desktop.WinUI3;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public sealed partial class App : Application
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitive Typen & Strings ──
    private const string AppSettingsFileName = "appsettings.json";
    private const string DefaultLanguageCode = "en-US";
    private const string DefaultThemeName = "Light";

    // ═══════════════════════════════════════════════════════
    //  4. Properties
    // ═══════════════════════════════════════════════════════
    // ── Block 1: Injizierte Abhängigkeiten (Dependencies) ──
    public static IHost? AppHost { get; private set; }

    // ── Block 4: Komplexe Typen, Collections & UI-Elemente ──
    public static Microsoft.UI.Dispatching.DispatcherQueue? AppDispatcherQueue { get; private set; }
    public static Window? MainWindoweBRestarter { get; private set; }


    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Initializes the singleton application object.
    /// </summary>
    public App()
    {
        InitializeComponent();

        AppHost = CreateHostBuilder().Build();
    }


    // ═══════════════════════════════════════════════════════
    //  8. Methods (public → private)
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        StartupDisplayPreferences? launchConfig = null;

        try
        {
            launchConfig = AppHost!.Services.GetRequiredService<IInboundPortStartupConfigProvider>().RetrieveStartupPreferences();
            _ = AppHost!.Services.GetRequiredService<IUseCaseInitializeBrowserCleanup>().ExecuteAsync();
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
            ApplicationLanguages.PrimaryLanguageOverride = DefaultLanguageCode;
        }

        try
        {
            var restartScheduler = AppHost!.Services.GetRequiredService<IInboundPortComputerRestartService>();
            restartScheduler.StartScheduler();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Fehler beim Starten des ComputerRestartSchedulers: {ex.Message}");
        }

        MainWindoweBRestarter = AppHost!.Services.GetRequiredService<EBRestarter>();
        AppDispatcherQueue = MainWindoweBRestarter.DispatcherQueue;

        try
        {
            var themeService = AppHost!.Services.GetRequiredService<IThemeHandler>();
            string themeToSet = launchConfig != null && !string.IsNullOrEmpty(launchConfig.ThemeName)
                ? launchConfig.ThemeName
                : DefaultThemeName;

            themeService.SetTheme(themeToSet);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Fehler beim Laden des Themes: {ex.Message}");
        }

        MainWindoweBRestarter.Activate();
    }

    private static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((_, cfg) =>
            {
                cfg.Sources.Clear();
                cfg.SetBasePath(AppContext.BaseDirectory);
                cfg.AddJsonFile(AppSettingsFileName, optional: false, reloadOnChange: true);
            })
            .ConfigureServices((_, services) =>
            {
                services.AddInfrastructureServices();
                services.AddApplicationServices();
                services.AddNavigationService();
                services.AddDialogServiceExtensions();
                services.AddThemeHandler();

                services.AddViewModels();

                services.AddSingleton<ILanguageHandler, LanguageHandler>();
                services.AddSingleton<IInboundPortLocalizationProvider, LocalizationProvider>();
                services.AddSingleton<IUIOptionsProvider>(sp => (LocalizationProvider)sp.GetRequiredService<IInboundPortLocalizationProvider>());
                services.AddSingleton<IIconCreditProvider, IconCreditProvider>();
                services.AddSingleton<IBrowserDisplayNameResolverUtility, BrowserDisplayNameResolverUtility>();

                services.AddSingleton<IMainWindowProvider, MainWindowProvider>();
                services.AddSingleton<IAppWindowHelper, AppWindowHelper>();
            });
    }
}