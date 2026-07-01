using eBRestarter.Core.Application.Ports.Inbound.UseCases.InitializeBrowserCleanup;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Core.Application.Providers;
using eBRestarter.Core.Application.Extensions.DependencyInjections;
using eBRestarter.Core.Application.Ports.Inbound.Providers;
using eBRestarter.Core.Application.Ports.Inbound.Services;
using eBRestarter.Core.Application.Models;
using eBRestarter.Core.Application.Ports.Outbound.Application;
using eBRestarter.Desktop.WinUI3.Extensions.DependencyInjections;
using eBRestarter.Desktop.WinUI3.Helpers;
using eBRestarter.Desktop.WinUI3.Helpers.Interfaces;
using eBRestarter.Desktop.WinUI3.Providers;
using eBRestarter.Desktop.WinUI3.Providers.Interfaces;
using eBRestarter.Desktop.WinUI3.Services;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using eBRestarter.Infrastructure.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.Windows.Globalization;
using System;
using System.Globalization;
using System.Threading;


namespace eBRestarter.Desktop.WinUI3;

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

             services.AddInfrastructureServices();
             services.AddApplicationServices();
             services.AddNavigationService();
             services.AddDialogServiceExtensions();
             services.AddThemeHandler();

             services.AddViewModels();

             services.AddSingleton<ILanguageService, LanguageService>();
             services.AddSingleton<ILocalizationProvider, LocalizationProvider>();
             services.AddSingleton<IUIOptionsProvider>(sp => (LocalizationProvider)sp.GetRequiredService<ILocalizationProvider>());
             services.AddSingleton<IIconCreditProvider, IconCreditProvider>();
             services.AddSingleton<eBRestarter.Desktop.WinUI3.Utilities.IBrowserDisplayNameResolverUtility, eBRestarter.Desktop.WinUI3.Utilities.BrowserDisplayNameResolverUtility>();

             services.AddSingleton<IMainWindowProvider, MainWindowProvider>();
             services.AddSingleton<IAppWindowHelper, AppWindowHelper>();

         });

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        StartupDisplayPreferences? launchConfig = null;
        try
        {
            launchConfig = AppHost!.Services.GetRequiredService<IStartupConfigProvider>().RetrieveStartupPreferences();
            _ = AppHost!.Services.GetRequiredService<IInitializeBrowserCleanupUseCase>().ExecuteAsync();
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
            var restartScheduler = AppHost!.Services.GetRequiredService<IComputerRestartService>();
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

        MainWindoweBRestarter.Activate();
    }
}








