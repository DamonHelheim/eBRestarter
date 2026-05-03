using eBRestarter.Desktop.WinUI3.Services;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace eBRestarter.Desktop.WinUI3.DependencyInjections;

/// <summary>
/// Extension methods to register dialog services with the DI container.
/// </summary>
public static class DialoglServiceExtensions
{
    public static IServiceCollection AddDialoglServiceExtensions(this IServiceCollection services)
    {
        services.AddSingleton<IDialogService, DialogService>();
        return services;
    }
}
