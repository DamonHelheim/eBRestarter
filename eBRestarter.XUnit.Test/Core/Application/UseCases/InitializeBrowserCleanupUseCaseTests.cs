using NSubstitute;
using Shouldly;
using Xunit;
using System;
using System.Threading.Tasks;
using eBRestarter.Core.Application.ObjectArchetypes.Constants;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;
using eBRestarter.Core.Application.UseCases;
using eBRestarter.Core.Domain.ValueObjects;

namespace eBRestarter.XUnit.Test.Core.Application.UseCases;

public class InitializeBrowserCleanupUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhenNextDeleteIsToday_RollsForwardAndSaves()
    {
        var browser = new BrowserConfig();
        browser.UpdateCleanupSettings(7, TimeProvider.System);
        browser.SetNextCleanupDate(DateTime.Today);
        var config = new AppConfig
        {
            Browser = browser
        };

        var mockConfigService = Substitute.For<IOutboundPortEVisitorConfigRepository>();
        mockConfigService.LoadConfig().Returns(config);

        var sut = new InitializeBrowserCleanupUseCase(mockConfigService, TimeProvider.System);

        await sut.ExecuteAsync();

        browser.NextBrowserDeleteCacheDate.ShouldBe(DateTime.Today.AddDays(7));
        mockConfigService.Received(1).SaveConfig(Arg.Is<AppConfig>(c => c.Browser.NextBrowserDeleteCacheDate == DateTime.Today.AddDays(7)));
    }
}
