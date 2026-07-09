using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;
using eBRestarter.Infrastructure.Adapters.Outbound.Browsers;
using Microsoft.Extensions.DependencyInjection;

namespace eBRestarter.Infrastructure.Factories;

public sealed class BrowserFactory(IServiceProvider serviceProvider) : IOutboundPortBrowserFactory
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public IOutboundPortBrowser Create(BrowserType type)
    {
        return type switch
        {
            BrowserType.Chrome => _serviceProvider.GetRequiredService<AdapterChromeBrowser>(),
            BrowserType.Firefox => _serviceProvider.GetRequiredService<AdapterFirefoxBrowser>(),
            BrowserType.Edge => _serviceProvider.GetRequiredService<AdapterEdgeBrowser>(),
            BrowserType.Brave => _serviceProvider.GetRequiredService<AdapterBraveBrowser>(),
            BrowserType.Vivaldi => _serviceProvider.GetRequiredService<AdapterVivaldiBrowser>(),
            _ => throw new NotSupportedException($"Browser {type} ist noch nicht implementiert.")
        };
    }
}







