using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace eBRestarter.Core.Application.DependencyInjections;

public static class ApplicationServiceExtensions
{
    // =========================================================
    // 1. PUBLIC & PROTECTED METHODS (API / Extension)
    // =========================================================
    #region PublicAndProtectedMethods

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IComputerRestartScheduler, ComputerRestartScheduler>();
        services.AddSingleton<IRestartCalculationService, RestartCalculationService>();
        services.AddSingleton<ICacheDeletionIntervalValidator, CacheDeletionIntervalValidator>();
        services.AddSingleton<IBrowserDisplayNameResolver, BrowserDisplayNameResolverService>();
        services.AddSingleton<IBrowserCleanupScheduleService, BrowserCleanupScheduleService>();
        services.AddSingleton<IRestartTaskDisplayStateService, RestartTaskDisplayStateService>();
        return services;
    }

    #endregion
}
