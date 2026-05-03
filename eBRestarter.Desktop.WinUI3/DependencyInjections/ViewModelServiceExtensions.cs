using eBRestarter.Desktop.WinUI3.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace eBRestarter.Desktop.WinUI3.DependencyInjections;

/// <summary>
/// Extension methods to register ViewModels and main window with the DI container.
/// </summary>
public static class ViewModelServiceExtensions
{
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddSingleton<EBRestarter>();
        services.AddSingleton<ViewModelRestartTask>();
        services.AddSingleton<ViewModelRestarterProperties>();
        services.AddSingleton<ViewModelGeneralOverview>();
        services.AddSingleton<ViewModelInstalledBrowsers>();
        services.AddSingleton<ViewModelOptions>();
        services.AddSingleton<ViewModelInfocenter>();
        services.AddTransient<ViewModelAbout>();

        services.AddTransient<ViewModelNetworkTraffic>();
        services.AddTransient<ViewModelDeleteBrowserContent>();
        services.AddTransient<ViewModelInstallAddOn>();
        services.AddTransient<ViewModelActivateApi>();
        services.AddTransient<ViewModelImportApi>();
        services.AddTransient<ViewModelTurnOffEdgeStartupBoost>();

        return services;
    }
}
