using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Domain.Enums;
using eBRestarter.Infrastructure.Browsers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Infrastructure.Factories
{
    public class BrowserFactory : IBrowserFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public BrowserFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IBrowser Create(BrowserType type)
        {
            return type switch
            {
                BrowserType.Chrome => _serviceProvider.GetRequiredService<ChromeBrowser>(),
                BrowserType.Firefox => _serviceProvider.GetRequiredService<FirefoxBrowser>(),
                BrowserType.Edge => _serviceProvider.GetRequiredService<EdgeBrowser>(),
                BrowserType.Brave => _serviceProvider.GetRequiredService<BraveBrowser>(),
                _ => throw new NotSupportedException($"Browser {type} ist noch nicht implementiert.")
            };
        }
    }
}
