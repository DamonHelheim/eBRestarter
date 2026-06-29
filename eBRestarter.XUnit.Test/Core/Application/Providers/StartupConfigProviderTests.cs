using eBRestarter.Core.Application.Ports.Outbound.Providers;
using eBRestarter.Core.Application.Providers;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Core.Application.Ports.Outbound.Application;
using eBRestarter.Core.Application.Ports.Outbound.Config;
using eBRestarter.Core.Domain.Entities;
using Moq;
using Shouldly;
using Xunit;

namespace eBRestarter.XUnit.Test.Core.Application.Providers;

public class StartupConfigProviderTests
{
    [Fact]
    public void PrepareConfigForLaunch_WhenNextDeleteIsToday_RollsForwardAndReturnsPreferences()
    {
        var browser = new BrowserConfig();
        browser.UpdateCleanupSettings(7, TimeProvider.System);
        browser.SetNextCleanupDate(DateTime.Today);
        var config = new AppConfig
        {
            Browser = browser,
            Settings = new SettingsConfig { Language = 0, Theme = "Dark" }
        };

        var mockConfigService = new Mock<IEVisitorConfigPort>();
        mockConfigService.Setup(s => s.LoadConfig()).Returns(config);

        var sut = new StartupConfigProvider(mockConfigService.Object);

        var prefs = sut.PrepareConfigForLaunch();

        prefs.LanguageCode.ShouldBe("de-DE");
        prefs.ThemeName.ShouldBe("Dark");
        browser.NextBrowserDeleteCacheDate.ShouldBe(DateTime.Today.AddDays(7));
        mockConfigService.Verify(s => s.SaveConfig(It.Is<AppConfig>(c => c.Browser.NextBrowserDeleteCacheDate == DateTime.Today.AddDays(7))), Times.Once);
    }
}







