using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Infrastructure.Services.WindowsOS;
using Moq;
using Shouldly;
using System.IO;
using Xunit;

namespace eBRestarter.XUnit.Test.Infrastructure.Services;

public class WindowsPathServiceTests
{
    private readonly Mock<IPathProvider> _mockPathProvider;
    private readonly WindowsPathService _sut;

    public WindowsPathServiceTests()
    {
        _mockPathProvider = new Mock<IPathProvider>();
        _sut = new WindowsPathService(_mockPathProvider.Object);
    }

    [Fact]
    public void GetAppDataPath_ShouldReturnCorrectPath()
    {
        // Arrange
        var localAppData = @"C:\Users\Test\AppData\Local";
        _mockPathProvider.Setup(p => p.GetLocalAppDataDirectory()).Returns(localAppData);

        // Act
        var result = _sut.GetAppDataPath();

        // Assert
        result.ShouldBe(Path.Combine(localAppData, "Skylar", "eBRestarter"));
    }

    [Fact]
    public void GetDownloadsPath_ShouldReturnCorrectPath()
    {
        // Arrange
        var userProfile = @"C:\Users\Test";
        _mockPathProvider.Setup(p => p.GetUserProfileDirectory()).Returns(userProfile);

        // Act
        var result = _sut.GetDownloadsPath();

        // Assert
        result.ShouldBe(Path.Combine(userProfile, "Downloads"));
    }

    [Fact]
    public void GetConfigFilePath_ShouldReturnCorrectPath()
    {
        // Arrange
        var localAppData = @"C:\Users\Test\AppData\Local";
        _mockPathProvider.Setup(p => p.GetLocalAppDataDirectory()).Returns(localAppData);

        // Act
        var result = _sut.GetConfigFilePath();

        // Assert
        result.ShouldBe(Path.Combine(localAppData, "Skylar", "eBRestarter", "eBRestarterConfig.json"));
    }

    [Fact]
    public void GetLogFilePath_ShouldReturnCorrectPath()
    {
        // Arrange
        var localAppData = @"C:\Users\Test\AppData\Local";
        _mockPathProvider.Setup(p => p.GetLocalAppDataDirectory()).Returns(localAppData);

        // Act
        var result = _sut.GetLogFilePath();

        // Assert
        result.ShouldBe(Path.Combine(localAppData, "Skylar", "eBRestarter", "log.txt"));
    }
}