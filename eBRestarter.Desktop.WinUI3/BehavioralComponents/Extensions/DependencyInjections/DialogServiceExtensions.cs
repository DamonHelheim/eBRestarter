using eBRestarter.Desktop.WinUI3.BehavioralComponents.Services;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Extensions.DependencyInjections;

/// <summary>
/// Extension methods to register dialog services with the DI container.
/// </summary>
public static class DialogServiceExtensions
{
    public static IServiceCollection AddDialogServiceExtensions(this IServiceCollection services)
    {
        services.AddSingleton<IDialogService, DialogService>();
        return services;
    }
}

