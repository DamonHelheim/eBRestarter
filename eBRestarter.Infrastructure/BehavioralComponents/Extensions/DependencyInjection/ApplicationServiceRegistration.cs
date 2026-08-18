using eBRestarter.Core.Application.BehavioralComponents.Handlers;
using eBRestarter.Core.Application.BehavioralComponents.Handlers.Interfaces;
using eBRestarter.Core.Application.BehavioralComponents.Providers;
using eBRestarter.Core.Application.BehavioralComponents.Services;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Handlers;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Services;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;
using eBRestarter.Core.Application.UseCases;
using eBRestarter.Core.Domain.Handlers;
using eBRestarter.Core.Domain.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace eBRestarter.Infrastructure.BehavioralComponents.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering Application core services and handlers into the dependency injection container.
/// </summary>
public static class ApplicationServiceRegistration
{
    /// <summary>
    /// Registers application domain handlers, use cases, providers, and validators with the DI container.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The same service collection for method chaining.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IDelayPhaseHandler, DelayPhaseHandler>();
        services.AddSingleton<IRunBrowserPhaseHandler, RunBrowserPhaseHandler>();

        services.AddSingleton<IRestartCalculationHandler, RestartCalculationHandler>();
        services.AddSingleton<IBrowserCleanupScheduleHandler, BrowserCleanupScheduleHandler>();

        services.AddSingleton<IInboundPortNextRestartDateHandler, NextRestartDateHandler>();
        services.AddSingleton<IInboundPortStartupConfigProvider, StartupConfigProvider>();
        services.AddSingleton<IInboundPortRestartTaskDisplayStateHandler, RestartTaskDisplayStateHandler>();
        services.AddSingleton<IInboundPortSystemInformationProvider, SystemInformationProvider>();

        services.AddTransient<IUseCaseDeleteBrowserContent, DeleteBrowserContentUseCase>();
        services.AddTransient<IUseCaseInitializeBrowserCleanup, InitializeBrowserCleanupUseCase>();
        services.AddSingleton<IInboundPortRestarterCycleService, RestarterCycleService>();
        services.AddSingleton<IInboundPortComputerRestartService, ComputerRestartService>();
        services.AddSingleton<IUseCaseConfigureAutoLogon, ConfigureAutoLogonUseCase>();
        services.AddSingleton<IUseCaseToggleAppAutoStart, ToggleAppAutoStartUseCase>();
        services.AddTransient<IUseCaseToggleEdgeStartupBoost, ToggleEdgeStartupBoostUseCase>();
        services.AddSingleton<IUseCaseManageApplicationUpdates, ManageApplicationUpdatesUseCase>();
        services.AddSingleton<IUseCaseDownloadBrowser, DownloadBrowserUseCase>();
        services.AddSingleton<IUseCaseScheduleBrowserCleanup, ScheduleBrowserCleanupUseCase>();
        services.AddSingleton<IUseCaseRemoveApiCredentials, RemoveApiCredentialsUseCase>();

        services.AddSingleton<ICacheDeletionIntervalValidator, CacheDeletionIntervalValidator>();

        return services;
    }
}
