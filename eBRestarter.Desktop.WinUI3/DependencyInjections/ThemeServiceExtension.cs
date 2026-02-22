using eBRestarter.Desktop.WinUI3.Services;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace eBRestarter.Desktop.WinUI3.DependencyInjections;

/// <summary>
/// Extension methods to register theme-related services with the DI container.
/// </summary>
public static class ThemeServiceExtension
{
    // =========================================================
    // 1. PUBLIC METHODS (API / Extension)
    // =========================================================
    #region PublicMethods

    public static IServiceCollection AddThemeService(this IServiceCollection services)
    {
        services.AddSingleton<IThemeService, ThemeService>();
        return services;
    }

    #endregion
}
