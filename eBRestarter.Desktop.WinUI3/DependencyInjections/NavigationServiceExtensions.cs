using eBRestarter.Desktop.WinUI3.Services;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using eBRestarter.Desktop.WinUI3.Views.Pages;
using Microsoft.Extensions.DependencyInjection;

namespace eBRestarter.Desktop.WinUI3.DependencyInjections;

/// <summary>
/// Extension methods to register the navigation service and routes with the DI container.
/// </summary>
public static class NavigationServiceExtensions
{
    // =========================================================
    // 1. PUBLIC METHODS (API / Extension)
    // =========================================================
    #region PublicMethods

    public static IServiceCollection AddNavigationService(this IServiceCollection services)
    {
        services.AddSingleton<INavigationService>(provider =>
        {
            var navService = new NavigationService();
            navService.RegisterRoute("CommonOverview", typeof(P_CommonOverview));
            navService.RegisterRoute("RestarterProperties", typeof(P_RestarterProperties));
            navService.RegisterRoute("Options", typeof(P_Options));
            navService.RegisterRoute("Infocenter", typeof(P_Infocenter));
            navService.RegisterRoute("Settings", typeof(P_Settings));
            return navService;
        });
        return services;
    }

    #endregion
}
