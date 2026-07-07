using eBRestarter.Core.Application.Handlers;
using eBRestarter.Core.Application.Ports.Inbound.Handlers;
using eBRestarter.Core.Application.Ports.Inbound.Providers;
using eBRestarter.Core.Application.Ports.Inbound.Services;
using eBRestarter.Core.Application.Ports.Inbound.UseCases;
using eBRestarter.Core.Application.Ports.Inbound.UseCases.ManageApplicationUpdates;
using eBRestarter.Core.Application.Ports.Inbound.UseCases.ScheduleBrowserCleanup;
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

        services.AddTransient<IDeleteBrowserContentUseCase, DeleteBrowserContentUseCase>();
        services.AddTransient<IInitializeBrowserCleanupUseCase, InitializeBrowserCleanupUseCase>();
        services.AddTransient<IRestarterCycleService, RestarterCycleService>();
        services.AddSingleton<IComputerRestartService, ComputerRestartService>();
        services.AddTransient<IConfigureAutoLogonUseCase, ConfigureAutoLogonUseCase>();
        services.AddTransient<IToggleAppAutoStartUseCase, ToggleAppAutoStartUseCase>();
        services.AddTransient<IToggleEdgeStartupBoostUseCase, ToggleEdgeStartupBoostUseCase>();
        services.AddTransient<IManageApplicationUpdatesUseCase, ManageApplicationUpdatesUseCase>();
        services.AddTransient<IDownloadBrowserUseCase, DownloadBrowserUseCase>();
        services.AddTransient<IScheduleBrowserCleanupUseCase, ScheduleBrowserCleanupUseCase>();
        services.AddTransient<IRemoveApiCredentialsUseCase, RemoveApiCredentialsUseCase>();

        services.AddSingleton<ICacheDeletionIntervalValidator, CacheDeletionIntervalValidator>();

        return services;
    }
}









