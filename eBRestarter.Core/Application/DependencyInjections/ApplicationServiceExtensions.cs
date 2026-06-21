#pragma warning disable T0046
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Services;
using eBRestarter.Core.Application.UseCases.ConfigureAutoLogon;
using eBRestarter.Core.Application.UseCases.DeleteBrowserContent;
using eBRestarter.Core.Application.UseCases.GetSystemInformation;
using eBRestarter.Core.Application.UseCases.ManageApplicationUpdates;
using eBRestarter.Core.Application.UseCases.ManageRestarterCycle;
using eBRestarter.Core.Application.UseCases.RemoveApiCredentials;
using eBRestarter.Core.Application.UseCases.ScheduleBrowserCleanup;
using eBRestarter.Core.Application.UseCases.ToggleAppAutoStart;
using eBRestarter.Core.Application.UseCases.ToggleEdgeStartupBoost;
using eBRestarter.Core.Domain.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace eBRestarter.Core.Application.DependencyInjections;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IRestartCalculationHandler, RestartCalculationHandler>();
        services.AddSingleton<IRetrieveNextRestartDateUseCase, RetrieveNextRestartDateUseCase>();
        services.AddSingleton<IPrepareConfigForLaunchUseCase, PrepareConfigForLaunchUseCase>();
        services.AddSingleton<ICacheDeletionIntervalValidator, CacheDeletionIntervalValidator>();
        services.AddSingleton<IBrowserCleanupScheduleHandler, BrowserCleanupScheduleHandler>();
        services.AddSingleton<IRestartTaskDisplayStateUseCase, RestartTaskDisplayStateUseCase>();

        // Use Cases
        services.AddTransient<IDeleteBrowserContentUseCase, DeleteBrowserContentUseCase>();
        services.AddTransient<IManageRestarterCycleUseCase, ManageRestarterCycleUseCase>();
        services.AddTransient<IConfigureAutoLogonUseCase, ConfigureAutoLogonUseCase>();
        services.AddTransient<IToggleAppAutoStartUseCase, ToggleAppAutoStartUseCase>();
        services.AddTransient<IToggleEdgeStartupBoostUseCase, ToggleEdgeStartupBoostUseCase>();
        services.AddTransient<IManageApplicationUpdatesUseCase, ManageApplicationUpdatesUseCase>();
        services.AddTransient<IScheduleBrowserCleanupUseCase, ScheduleBrowserCleanupUseCase>();
        services.AddTransient<IGetSystemInformationUseCase, GetSystemInformationUseCase>();
        services.AddTransient<IRemoveApiCredentialsUseCase, RemoveApiCredentialsUseCase>();

        services.AddValidatorsFromAssemblyContaining<ConfigureAutoLogonRequestValidator>();

        return services;
    }
}
