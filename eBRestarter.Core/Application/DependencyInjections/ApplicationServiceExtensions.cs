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
using Microsoft.Extensions.DependencyInjection;

namespace eBRestarter.Core.Application.DependencyInjections;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IRestartCalculationService, RestartCalculationService>();
        services.AddSingleton<IComputerRestartDateService, ComputerRestartDateService>();
        services.AddSingleton<IApplicationLaunchConfigService, ApplicationLaunchConfigService>();
        services.AddSingleton<ICacheDeletionIntervalValidator, CacheDeletionIntervalValidator>();
        services.AddSingleton<IBrowserCleanupScheduleService, BrowserCleanupScheduleService>();
        services.AddSingleton<IRestartTaskDisplayStateService, RestartTaskDisplayStateService>();

        // Use Cases
        services.AddTransient<IDeleteBrowserContentUseCase, DeleteBrowserContentService>();
        services.AddTransient<IManageRestarterCycleUseCase, ManageRestarterCycleService>();
        services.AddTransient<IConfigureAutoLogonUseCase, ConfigureAutoLogonService>();
        services.AddTransient<IToggleAppAutoStartUseCase, ToggleAppAutoStartService>();
        services.AddTransient<IToggleEdgeStartupBoostUseCase, ToggleEdgeStartupBoostService>();
        services.AddTransient<IManageApplicationUpdatesUseCase, ManageApplicationUpdatesService>();
        services.AddTransient<IScheduleBrowserCleanupUseCase, ScheduleBrowserCleanupService>();
        services.AddTransient<IGetSystemInformationUseCase, GetSystemInformationService>();
        services.AddTransient<IRemoveApiCredentialsUseCase, RemoveApiCredentialsService>();

        return services;
    }
}
