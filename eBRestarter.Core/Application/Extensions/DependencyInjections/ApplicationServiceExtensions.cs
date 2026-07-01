using eBRestarter.Core.Application.Handlers.ManageRestarterCycle;
// Removed Strategies namespace
using eBRestarter.Core.Application.Ports.Inbound.Handlers;
using eBRestarter.Core.Application.Ports.Inbound.Providers;
using eBRestarter.Core.Application.Ports.Inbound.UseCases.ConfigureAutoLogon;
using eBRestarter.Core.Application.Ports.Inbound.UseCases.DeleteBrowserContent;
using eBRestarter.Core.Application.Ports.Inbound.UseCases.DownloadBrowser;
using eBRestarter.Core.Application.Ports.Inbound.UseCases.ManageApplicationUpdates;
using eBRestarter.Core.Application.Ports.Inbound.Services;
using eBRestarter.Core.Application.Ports.Inbound.UseCases.RemoveApiCredentials;
using eBRestarter.Core.Application.Ports.Inbound.UseCases.ScheduleBrowserCleanup;
using eBRestarter.Core.Application.Ports.Inbound.UseCases.ToggleAppAutoStart;
using eBRestarter.Core.Application.Ports.Inbound.UseCases.ToggleEdgeStartupBoost;
using eBRestarter.Core.Application.Handlers;
using eBRestarter.Core.Application.Providers;
using eBRestarter.Core.Application.Providers.SystemInfo;
using eBRestarter.Core.Application.UseCases.ConfigureAutoLogon;
using eBRestarter.Core.Application.UseCases.DeleteBrowserContent;
using eBRestarter.Core.Application.UseCases.DownloadBrowser;
using eBRestarter.Core.Application.UseCases.ManageApplicationUpdates;
using eBRestarter.Core.Application.Services;
using eBRestarter.Core.Application.UseCases.RemoveApiCredentials;
using eBRestarter.Core.Application.UseCases.ScheduleBrowserCleanup;
using eBRestarter.Core.Application.UseCases.ToggleAppAutoStart;
using eBRestarter.Core.Application.UseCases.ToggleEdgeStartupBoost;
using eBRestarter.Core.Application.Validators;
using eBRestarter.Core.Domain.Handlers;
using eBRestarter.Core.Domain.Validators;
using FluentValidation;
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

        services.AddSingleton<INextRestartDateHandler, NextRestartDateHandler>();
        services.AddSingleton<IStartupConfigProvider, StartupConfigProvider>();
        services.AddSingleton<IRestartTaskDisplayStateHandler, RestartTaskDisplayStateHandler>();
        services.AddTransient<ISystemInformationProvider, SystemInformationProvider>();

        services.AddTransient<IDeleteBrowserContentUseCase, DeleteBrowserContentUseCase>();
        services.AddTransient<eBRestarter.Core.Application.Ports.Inbound.UseCases.InitializeBrowserCleanup.IInitializeBrowserCleanupUseCase, eBRestarter.Core.Application.UseCases.InitializeBrowserCleanup.InitializeBrowserCleanupUseCase>();
        services.AddTransient<eBRestarter.Core.Application.Ports.Inbound.Services.IRestarterCycleService, eBRestarter.Core.Application.Services.RestarterCycleService>();
        services.AddSingleton<IComputerRestartService, ComputerRestartService>();
        services.AddTransient<IConfigureAutoLogonUseCase, ConfigureAutoLogonUseCase>();
        services.AddTransient<IToggleAppAutoStartUseCase, ToggleAppAutoStartUseCase>();
        services.AddTransient<IToggleEdgeStartupBoostUseCase, ToggleEdgeStartupBoostUseCase>();
        services.AddTransient<IManageApplicationUpdatesUseCase, ManageApplicationUpdatesUseCase>();
        services.AddTransient<IDownloadBrowserUseCase, DownloadBrowserUseCase>();
        services.AddTransient<IScheduleBrowserCleanupUseCase, ScheduleBrowserCleanupUseCase>();
        services.AddTransient<IRemoveApiCredentialsUseCase, RemoveApiCredentialsUseCase>();

        services.AddSingleton<ICacheDeletionIntervalValidator, CacheDeletionIntervalValidator>();
        services.AddValidatorsFromAssemblyContaining<ConfigureAutoLogonValidator>();

        return services;
    }
}









