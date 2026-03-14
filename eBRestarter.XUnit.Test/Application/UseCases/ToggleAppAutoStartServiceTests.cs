using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Application.UseCases.ToggleAppAutoStart;
using eBRestarter.Core.Domain.Models.Records.Config;
using Moq;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace eBRestarter.XUnit.Test.Application.UseCases;

public class ToggleAppAutoStartServiceTests
{
    private readonly Mock<IWindowsStartupManagerService> _mockStartupManagerService;
    private readonly Mock<IEVisitorConfigService> _mockConfigService;
    private readonly ToggleAppAutoStartService _sut;

    public ToggleAppAutoStartServiceTests()
    {
        _mockStartupManagerService = new Mock<IWindowsStartupManagerService>();
        _mockConfigService = new Mock<IEVisitorConfigService>();
        _sut = new ToggleAppAutoStartService(_mockStartupManagerService.Object, _mockConfigService.Object);
    }

    [Fact]
    public async Task InitializeAndGetStateAsync_WhenEnabledInConfigButNotOs_ShouldEnableAndReturnTrue()
    {
        // Arrange
        var config = new AppConfig();
        config.Settings.StartWithWindows = true;
        
        _mockConfigService.Setup(c => c.LoadConfig()).Returns(config);
        _mockStartupManagerService.Setup(s => s.IsAutoStartEnabledAsync()).ReturnsAsync(false);

        // Act
        var result = await _sut.InitializeAndGetStateAsync();

        // Assert
        result.ShouldBeTrue();
        _mockStartupManagerService.Verify(s => s.EnableAutoStartAsync(), Times.Once);
    }

    [Fact]
    public async Task InitializeAndGetStateAsync_WhenEnabledInOs_ShouldReturnTrueAndNotCallEnable()
    {
        // Arrange
        var config = new AppConfig();
        config.Settings.StartWithWindows = true;
        
        _mockConfigService.Setup(c => c.LoadConfig()).Returns(config);
        _mockStartupManagerService.Setup(s => s.IsAutoStartEnabledAsync()).ReturnsAsync(true);

        // Act
        var result = await _sut.InitializeAndGetStateAsync();

        // Assert
        result.ShouldBeTrue();
        _mockStartupManagerService.Verify(s => s.EnableAutoStartAsync(), Times.Never);
    }

    [Fact]
    public async Task ToggleAsync_WhenEnableTrue_ShouldEnableInOsAndSaveConfig()
    {
        // Arrange
        var config = new AppConfig();
        _mockConfigService.Setup(c => c.LoadConfig()).Returns(config);

        // Act
        await _sut.ToggleAsync(true);

        // Assert
        _mockStartupManagerService.Verify(s => s.EnableAutoStartAsync(), Times.Once);
        config.Settings.StartWithWindows.ShouldBeTrue();
        _mockConfigService.Verify(c => c.SaveConfig(config), Times.Once);
    }

    [Fact]
    public async Task ToggleAsync_WhenEnableFalse_ShouldDisableInOsAndSaveConfig()
    {
        // Arrange
        var config = new AppConfig();
        config.Settings.StartWithWindows = true; // Initial state true
        _mockConfigService.Setup(c => c.LoadConfig()).Returns(config);

        // Act
        await _sut.ToggleAsync(false);

        // Assert
        _mockStartupManagerService.Verify(s => s.DisableAutoStartAsync(), Times.Once);
        config.Settings.StartWithWindows.ShouldBeFalse();
        _mockConfigService.Verify(c => c.SaveConfig(config), Times.Once);
    }
}
