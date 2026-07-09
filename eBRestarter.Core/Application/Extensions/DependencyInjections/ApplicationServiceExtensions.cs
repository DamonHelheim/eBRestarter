using eBRestarter.Core.Application.Handlers;
using eBRestarter.Core.Application.Handlers.Interfaces;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Handlers;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Services;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;
using eBRestarter.Core.Application.Providers;
using eBRestarter.Core.Application.Services;
using eBRestarter.Core.Application.UseCases;
using eBRestarter.Core.Domain.Handlers;
using eBRestarter.Core.Domain.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace eBRestarter.Core.Application.Extensions.DependencyInjections;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddTransient<IDelayPhaseHandler, DelayPhaseHandler>();
        services.AddTransient<IRunBrowserPhaseHandler, RunBrowserPhaseHandler>();

        services.AddSingleton<IRestartCalculationHandler, RestartCalculationHandler>();
        services.AddSingleton<IBrowserCleanupScheduleHandler, BrowserCleanupScheduleHandler>();

        services.AddSingleton<IInboundPortNextRestartDateHandler, NextRestartDateHandler>();
        services.AddSingleton<IInboundPortStartupConfigProvider, StartupConfigProvider>();
        services.AddSingleton<IInboundPortRestartTaskDisplayStateHandler, RestartTaskDisplayStateHandler>();
        services.AddTransient<IInboundPortSystemInformationProvider, SystemInformationProvider>();

        services.AddTransient<IUseCaseDeleteBrowserContent, DeleteBrowserContentUseCase>();
        services.AddTransient<IUseCaseInitializeBrowserCleanup, InitializeBrowserCleanupUseCase>();
        services.AddTransient<IInboundPortRestarterCycleService, RestarterCycleService>();
        services.AddSingleton<IInboundPortComputerRestartService, ComputerRestartService>();
        services.AddTransient<IUseCaseConfigureAutoLogon, ConfigureAutoLogonUseCase>();
        services.AddTransient<IUseCaseToggleAppAutoStart, ToggleAppAutoStartUseCase>();
        services.AddTransient<IUseCaseToggleEdgeStartupBoost, ToggleEdgeStartupBoostUseCase>();
        services.AddTransient<IUseCaseManageApplicationUpdates, ManageApplicationUpdatesUseCase>();
        services.AddTransient<IUseCaseDownloadBrowser, DownloadBrowserUseCase>();
        services.AddTransient<IUseCaseScheduleBrowserCleanup, ScheduleBrowserCleanupUseCase>();
        services.AddTransient<IUseCaseRemoveApiCredentials, RemoveApiCredentialsUseCase>();

        services.AddSingleton<ICacheDeletionIntervalValidator, CacheDeletionIntervalValidator>();

        return services;
    }
}









