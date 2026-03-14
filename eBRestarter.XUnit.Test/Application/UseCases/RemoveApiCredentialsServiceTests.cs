using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.UseCases.RemoveApiCredentials;
using eBRestarter.Core.Domain.Models.Records.Config;
using Moq;
using Shouldly;
using Xunit;

namespace eBRestarter.XUnit.Test.Application.UseCases;

public class RemoveApiCredentialsServiceTests
{
    private readonly Mock<IEVisitorConfigService> _mockConfigService;
    private readonly RemoveApiCredentialsService _sut;

    public RemoveApiCredentialsServiceTests()
    {
        _mockConfigService = new Mock<IEVisitorConfigService>();
        _sut = new RemoveApiCredentialsService(_mockConfigService.Object);
    }

    [Fact]
    public void Execute_ShouldClearCredentialsAndSaveConfig()
    {
        // Arrange
        var initialConfig = new AppConfig();
        initialConfig.Settings.ApiUsername = "testUser";
        initialConfig.Settings.ApiKey = "secretKey";

        _mockConfigService.Setup(c => c.LoadConfig()).Returns(initialConfig);

        AppConfig? savedConfig = null;
        _mockConfigService.Setup(c => c.SaveConfig(It.IsAny<AppConfig>())).Callback<AppConfig>(c => savedConfig = c);

        // Act
        _sut.Execute();

        // Assert
        _mockConfigService.Verify(c => c.LoadConfig(), Times.Once);
        _mockConfigService.Verify(c => c.SaveConfig(It.IsAny<AppConfig>()), Times.Once);

        savedConfig.ShouldNotBeNull();
        savedConfig!.Settings.ApiUsername.ShouldBeEmpty();
        savedConfig.Settings.ApiKey.ShouldBeEmpty();
    }
}
