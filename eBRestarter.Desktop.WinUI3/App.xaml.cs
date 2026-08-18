using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.Windows.Globalization;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using eBRestarter.Core.Application.ObjectArchetypes.Constants;
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
    // ── Block 2: Primitives & strings ──
    private const string AppSettingsFileName = "appsettings.json";
    private const string DefaultLanguageCode = "en-US";
    private const string DefaultThemeName = "Light";

    // ═══════════════════════════════════════════════════════
    //  4. Properties
    // ═══════════════════════════════════════════════════════
    // ── Block 1: Injected dependencies ──
    /// <summary>Gets the global application dependency injection host.</summary>
    public static IHost? AppHost { get; private set; }

    // ── Block 4: Complex types, collections & UI elements ──
    /// <summary>Gets the global dispatcher queue associated with the UI thread.</summary>
    public static Microsoft.UI.Dispatching.DispatcherQueue? AppDispatcherQueue { get; private set; }

    /// <summary>Gets the main application window instance.</summary>
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

        RegisterGlobalExceptionHandlers();
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
        // Logger is retrieved from host since App is instantiated by WinUI XAML framework, not DI container.
        var logger = AppHost!.Services.GetRequiredService<ILogger<App>>();

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
        catch (Exception ex)
        {
            // Fall back to default language when configuration loading fails.
            logger.LogWarning(
                LogEventIds.UserInterface.StartupConfigurationFailed,
                ex,
                "Loading the startup configuration failed; falling back to the default language.");

            ApplicationLanguages.PrimaryLanguageOverride = DefaultLanguageCode;
        }

        try
        {
            var restartScheduler = AppHost!.Services.GetRequiredService<IInboundPortComputerRestartService>();
            restartScheduler.StartScheduler();
        }
        catch (Exception ex)
        {
            logger.LogError(
                LogEventIds.UserInterface.SchedulerStartFailed,
                ex,
                "Starting the computer restart scheduler failed; scheduled restarts will not run.");
        }

        MainWindoweBRestarter = AppHost!.Services.GetRequiredService<EBRestarter>();
        AppDispatcherQueue = MainWindoweBRestarter.DispatcherQueue;

        // Teardown hook: Ensures singleton ViewModels disposing timers/handlers are properly cleaned up when the window closes.
        MainWindoweBRestarter.Closed += OnMainWindowClosed;

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
            logger.LogWarning(
                LogEventIds.UserInterface.ThemeLoadFailed,
                ex,
                "Applying the configured theme failed; the default theme stays active.");
        }

        MainWindoweBRestarter.Activate();
    }

    /// <summary>
    /// Wires up the two application-wide last-resort exception sinks.
    /// </summary>
    /// <remarks>
    /// Registers global exception sinks for unhandled UI exceptions and unobserved background tasks.
    /// Unhandled UI exceptions trigger a critical log entry before termination, while unobserved task exceptions
    /// are logged and marked as observed to prevent unexpected crashes from background operations.
    /// </remarks>
    private static void RegisterGlobalExceptionHandlers()
    {
        Current.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    /// <summary>Last-resort sink for exceptions that reach the UI thread unhandled.</summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="args">Unhandled exception event arguments.</param>
    private static void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs args)
    {
        var logger = AppHost?.Services.GetService<ILogger<App>>();

        logger?.LogCritical(
            LogEventIds.UserInterface.UnhandledUiException,
            args.Exception,
            "Unhandled exception reached the UI thread; the application is terminating.");
    }

    /// <summary>Last-resort sink for faulted tasks whose result was never observed.</summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="args">Unobserved task exception event arguments.</param>
    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs args)
    {
        var logger = AppHost?.Services.GetService<ILogger<App>>();

        // Flatten nested AggregateException instances so root causes are recorded in logs.
        foreach (var innerException in args.Exception.Flatten().InnerExceptions)
        {
            logger?.LogError(
                LogEventIds.RestarterCycle.FireAndForgetTaskFaulted,
                innerException,
                "A fire-and-forget task faulted and its result was never observed.");
        }

        // Mark task exception as observed to prevent process termination for background tasks.
        args.SetObserved();
    }

    /// <summary>
    /// Disposes the DI container when the main window closes, which in turn runs
    /// <see cref="IDisposable.Dispose"/> on every registered singleton (timer teardown,
    /// event unsubscription, messenger unregistration).
    /// </summary>
    /// <param name="sender">The closing window instance.</param>
    /// <param name="args">Window closing event arguments.</param>
    private static void OnMainWindowClosed(object sender, WindowEventArgs args)
    {
        // Resolve logger before disposing host container.
        var logger = AppHost?.Services.GetService<ILogger<App>>();

        try
        {
            if (sender is Window closedWindow)
            {
                closedWindow.Closed -= OnMainWindowClosed;
            }

            AppHost?.Dispose();
        }
        catch (Exception ex)
        {
            logger?.LogError(
                LogEventIds.UserInterface.HostDisposeFailed,
                ex,
                "Disposing the application host during shutdown failed.");
        }
    }

    /// <summary>Builds and configures the default application host and service container.</summary>
    private static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((_, cfg) =>
            {
                cfg.Sources.Clear();
                cfg.SetBasePath(AppContext.BaseDirectory);
                cfg.AddJsonFile(AppSettingsFileName, optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                // Configure application logging prior to other service registrations.
                services.AddApplicationLogging(context.Configuration);

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