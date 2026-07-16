using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;
using eBRestarter.Infrastructure.Adapters.Outbound.Browsers;
using Microsoft.Extensions.DependencyInjection;

namespace eBRestarter.Infrastructure.Factories;

/// <summary>
/// Enterprise-Standard .NET 10 Keyed Services DI-Factory gemäß Hexagonaler Architektur.
/// </summary>
public sealed class BrowserFactory(IServiceProvider serviceProvider) : IOutboundPortBrowserFactory
{
    public IOutboundPortBrowser Create(BrowserType type)
    {
        // Enterprise Keyed Services DI-Auflösung (.NET 10 Standard)
        if (serviceProvider.GetKeyedService<IOutboundPortBrowser>(type) is { } keyedBrowser)
        {
            return keyedBrowser;
        }

        // Fallback für Mocks & Legacy-Auflösung
        return type switch
        {
            BrowserType.Chrome => serviceProvider.GetRequiredService<AdapterChromeBrowser>(),
            BrowserType.Firefox => serviceProvider.GetRequiredService<AdapterFirefoxBrowser>(),
            BrowserType.Edge => serviceProvider.GetRequiredService<AdapterEdgeBrowser>(),
            BrowserType.Brave => serviceProvider.GetRequiredService<AdapterBraveBrowser>(),
            BrowserType.Vivaldi => serviceProvider.GetRequiredService<AdapterVivaldiBrowser>(),
            _ => throw new NotSupportedException($"Browser {type} ist noch nicht implementiert.")
        };
    }
}







