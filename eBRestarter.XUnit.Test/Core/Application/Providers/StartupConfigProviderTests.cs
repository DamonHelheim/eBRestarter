using eBRestarter.Core.Application.Ports.Inbound.Providers;
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
    public void RetrieveStartupPreferences_ReturnsCorrectPreferences_WithoutSaving()
    {
        var config = new AppConfig
        {
            Settings = new SettingsConfig { Language = 0, Theme = "Dark" }
        };

        var mockConfigService = new Mock<IEVisitorConfigRepositoryOutboundPort>();
        mockConfigService.Setup(s => s.LoadConfig()).Returns(config);

        var sut = new StartupConfigProvider(mockConfigService.Object);

        var prefs = sut.RetrieveStartupPreferences();

        prefs.LanguageCode.ShouldBe("de-DE");
        prefs.ThemeName.ShouldBe("Dark");
        mockConfigService.Verify(s => s.SaveConfig(It.IsAny<AppConfig>()), Times.Never);
    }
}







