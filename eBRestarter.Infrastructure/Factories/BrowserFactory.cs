using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Infrastructure.Browsers;
using Microsoft.Extensions.DependencyInjection;

namespace eBRestarter.Infrastructure.Factories;

public class BrowserFactory(IServiceProvider serviceProvider) : IBrowserFactory
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public IBrowser Create(BrowserType type)
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
