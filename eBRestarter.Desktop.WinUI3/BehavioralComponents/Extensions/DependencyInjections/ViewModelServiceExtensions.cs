using System;

using eBRestarter.Desktop.WinUI3.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Extensions.DependencyInjections;

/// <summary>
/// Extension methods to register ViewModels and main window with the DI container.
/// </summary>
public static class ViewModelServiceExtensions
{
    /// <summary>
    /// Registers all application ViewModels and ViewModel factory functions into the service collection.
    /// </summary>
    /// <param name="services">The service collection to populate.</param>
    /// <returns>The modified <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddSingleton<EBRestarter>();
        services.AddSingleton<ViewModelRestartTask>();
        services.AddSingleton<ViewModelRestarterProperties>();
        services.AddSingleton<ViewModelGeneralOverview>();
        services.AddSingleton<ViewModelInstalledBrowsers>();
        services.AddSingleton<ViewModelOptionsGeneral>();
        services.AddSingleton<ViewModelOptionsApi>();
        services.AddSingleton<ViewModelOptionsExtension>();
        services.AddSingleton<ViewModelInfocenter>();

        // Transient ViewModels without IDisposable implementation.
        services.AddTransient<ViewModelAbout>();
        services.AddTransient<ViewModelDeleteBrowserContent>();
        services.AddTransient<ViewModelActivateApi>();
        services.AddTransient<ViewModelTurnOffEdgeStartupBoost>();

        // ViewModels implementing IDisposable holding active timers are registered as factory functions
        // to prevent DI root container tracking memory leaks across dialog lifetimes.
        services.AddSingleton<Func<ViewModelNetworkTraffic>>(
            static serviceProvider => () => ActivatorUtilities.CreateInstance<ViewModelNetworkTraffic>(serviceProvider));

        services.AddSingleton<Func<ViewModelInstallAddOn>>(
            static serviceProvider => () => ActivatorUtilities.CreateInstance<ViewModelInstallAddOn>(serviceProvider));

        return services;
    }
}
