using eBRestarter.Core.Application.Ports.Outbound.Config;
using eBRestarter.Core.Application.UseCases.InitializeBrowserCleanup;
using eBRestarter.Core.Domain.Entities;
using Moq;
using Shouldly;
using Xunit;
using System;
using System.Threading.Tasks;

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

        var mockConfigService = new Mock<IEVisitorConfigRepositoryOutboundPort>();
        mockConfigService.Setup(s => s.LoadConfig()).Returns(config);

        var sut = new InitializeBrowserCleanupUseCase(mockConfigService.Object);

        await sut.ExecuteAsync();

        browser.NextBrowserDeleteCacheDate.ShouldBe(DateTime.Today.AddDays(7));
        mockConfigService.Verify(s => s.SaveConfig(It.Is<AppConfig>(c => c.Browser.NextBrowserDeleteCacheDate == DateTime.Today.AddDays(7))), Times.Once);
    }
}
