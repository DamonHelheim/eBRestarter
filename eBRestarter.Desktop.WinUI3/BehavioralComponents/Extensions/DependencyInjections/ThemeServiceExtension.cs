using eBRestarter.Desktop.WinUI3.BehavioralComponents.Handler;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Handler.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Extensions.DependencyInjections;

/// <summary>
/// Extension methods to register theme-related handlers with the DI container.
/// </summary>
public static class ThemeServiceExtension
{
    public static IServiceCollection AddThemeHandler(this IServiceCollection services)
    {
        services.AddSingleton<IThemeHandler, ThemeHandler>();
        return services;
    }
}
