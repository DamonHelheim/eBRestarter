using eBRestarter.Core.Application.Handlers.ManageRestarterCycle.Strategies;
using eBRestarter.Core.Application.Ports.Inbound.Providers;
using eBRestarter.Core.Application.Ports.Inbound.UseCases.ConfigureAutoLogon;
using eBRestarter.Core.Application.Ports.Inbound.UseCases.DeleteBrowserContent;
using eBRestarter.Core.Application.Ports.Inbound.UseCases.ManageApplicationUpdates;
using eBRestarter.Core.Application.Ports.Inbound.UseCases.ManageRestarterCycle;
using eBRestarter.Core.Application.Ports.Inbound.UseCases.RemoveApiCredentials;
using eBRestarter.Core.Application.Ports.Inbound.UseCases.ScheduleBrowserCleanup;
using eBRestarter.Core.Application.Ports.Inbound.UseCases.ToggleAppAutoStart;
using eBRestarter.Core.Application.Ports.Inbound.UseCases.ToggleEdgeStartupBoost;
using eBRestarter.Core.Application.Ports.Outbound.SystemInfo;
using eBRestarter.Core.Application.Providers;
using eBRestarter.Core.Application.Providers.SystemInfo;
using eBRestarter.Core.Application.UseCases.ConfigureAutoLogon;
using eBRestarter.Core.Application.UseCases.DeleteBrowserContent;
using eBRestarter.Core.Application.UseCases.ManageApplicationUpdates;
using eBRestarter.Core.Application.UseCases.ManageRestarterCycle;
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
        services.AddTransient<IDelayPhaseStrategy, DelayPhaseStrategy>();
        services.AddTransient<IRunBrowserPhaseStrategy, RunBrowserPhaseStrategy>();

        services.AddSingleton<IRestartCalculationHandler, RestartCalculationHandler>();
        services.AddSingleton<IBrowserCleanupScheduleHandler, BrowserCleanupScheduleHandler>();

        services.AddSingleton<INextRestartDateProvider, NextRestartDateProvider>();
        services.AddSingleton<IStartupConfigProvider, StartupConfigProvider>();
        services.AddSingleton<IRestartTaskDisplayStateProvider, RestartTaskDisplayStateProvider>();
        services.AddTransient<ISystemInformationPort, SystemInformationProvider>();

        services.AddTransient<IDeleteBrowserContentUseCase, DeleteBrowserContentUseCase>();
        services.AddTransient<IManageRestarterCycleUseCase, ManageRestarterCycleUseCase>();
        services.AddTransient<IConfigureAutoLogonUseCase, ConfigureAutoLogonUseCase>();
        services.AddTransient<IToggleAppAutoStartUseCase, ToggleAppAutoStartUseCase>();
        services.AddTransient<IToggleEdgeStartupBoostUseCase, ToggleEdgeStartupBoostUseCase>();
        services.AddTransient<IManageApplicationUpdatesUseCase, ManageApplicationUpdatesUseCase>();
        services.AddTransient<IScheduleBrowserCleanupUseCase, ScheduleBrowserCleanupUseCase>();
        services.AddTransient<IRemoveApiCredentialsUseCase, RemoveApiCredentialsUseCase>();

        services.AddSingleton<ICacheDeletionIntervalValidator, CacheDeletionIntervalValidator>();
        services.AddValidatorsFromAssemblyContaining<ConfigureAutoLogonValidator>();

        return services;
    }
}









