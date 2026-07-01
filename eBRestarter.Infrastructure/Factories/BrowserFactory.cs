using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Ports.Outbound.Browser;
using eBRestarter.Infrastructure.Adapters.Browsers;
using Microsoft.Extensions.DependencyInjection;

namespace eBRestarter.Infrastructure.Factories;

public sealed class BrowserFactory(IServiceProvider serviceProvider) : IBrowserFactoryOutboundPort
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public IBrowserOutboundPort Create(BrowserType type)
    {
        return type switch
        {
            BrowserType.Chrome => _serviceProvider.GetRequiredService<ChromeBrowser>(),
            BrowserType.Firefox => _serviceProvider.GetRequiredService<FirefoxBrowser>(),
            BrowserType.Edge => _serviceProvider.GetRequiredService<EdgeBrowser>(),
            BrowserType.Brave => _serviceProvider.GetRequiredService<BraveBrowser>(),
            BrowserType.Vivaldi => _serviceProvider.GetRequiredService<VivaldiBrowser>(),
            _ => throw new NotSupportedException($"Browser {type} ist noch nicht implementiert.")
        };
    }
}







