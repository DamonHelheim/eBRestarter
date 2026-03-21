using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Infrastructure.Browsers;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using System.IO;
using Xunit;

namespace eBRestarter.XUnit.Test.Infrastructure.Browsers;

public class ChromeBrowserTests
{
    private readonly Mock<IOperatingSystemFacade> _mockOsFacade;
    private readonly Mock<IWindowsProcessControlService> _mockProcessService;
    private readonly Mock<IWindowsRegistryService> _mockRegistryService;
    private readonly Mock<IWindowsFileSystemService> _mockFileSystemService;
    private readonly Mock<ILogger<ChromeBrowser>> _mockLogger;
    private readonly ChromeBrowser _sut;

    public ChromeBrowserTests()
    {
        _mockOsFacade = new Mock<IOperatingSystemFacade>();
        _mockProcessService = new Mock<IWindowsProcessControlService>();
        _mockRegistryService = new Mock<IWindowsRegistryService>();
        _mockFileSystemService = new Mock<IWindowsFileSystemService>();
        _mockLogger = new Mock<ILogger<ChromeBrowser>>();

        _mockOsFacade.Setup(os => os.WindowsProcessControlService).Returns(_mockProcessService.Object);
        _mockOsFacade.Setup(os => os.WindowsRegistryService).Returns(_mockRegistryService.Object);
        _mockOsFacade.Setup(os => os.WindowsFileSystemService).Returns(_mockFileSystemService.Object);

        // Basic setup for GetPaths() to not crash
        _mockFileSystemService.Setup(f => f.GetEnvironmentPath("LocalAppData")).Returns(@"C:\Users\Test\AppData\Local");
        _mockFileSystemService.Setup(f => f.GetEnvironmentPath("ProgramFiles")).Returns(@"C:\Program Files");
        _mockFileSystemService.Setup(f => f.GetEnvironmentPath("ProgramFiles(x86)")).Returns(@"C:\Program Files (x86)");
        _mockFileSystemService.Setup(f => f.CombinePaths(It.IsAny<string[]>())).Returns<string[]>(paths => Path.Combine(paths));

        _sut = new ChromeBrowser(_mockOsFacade.Object, _mockLogger.Object);
    }

    [Fact]
    public void Start_ShouldCallProcessServiceWithCorrectUrlAndArguments()
    {
        // Arrange
        var url = "https://www.google.com";
        var expectedExePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe";

        // Mock IsInstalled / ExecutablePaths
        _mockFileSystemService.Setup(f => f.FileExists(It.IsAny<string>())).Returns(false);
        _mockFileSystemService.Setup(f => f.FileExists(expectedExePath)).Returns(true);
        _mockFileSystemService.Setup(f => f.GetEnvironmentPath("ProgramFiles")).Returns(@"C:\Program Files");
        _mockFileSystemService.Setup(f => f.CombinePaths(It.IsAny<string[]>())).Returns<string[]>(paths => Path.Combine(paths));

        // Act
        _sut.Start(url);

        // Assert
        _mockProcessService.Verify(p => p.OpenUrlInBrowser(expectedExePath, url), Times.Once);
    }

    [Fact]
    public void Start_WhenNoExeFound_ShouldLogError()
    {
        // Arrange
        _mockFileSystemService.Setup(f => f.FileExists(It.IsAny<string>())).Returns(false); // Simulate uninstalled

        // Act
        // This won't throw because base.Start catches the exception
        _sut.Start("https://test.com");

        // Assert
        _mockProcessService.Verify(p => p.OpenUrlInBrowser(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Close_ShouldKillChromeProcess()
    {
        // Act
        _sut.Close();

        // Assert
        // The process name in ChromeBrowser is "chrome"
        _mockProcessService.Verify(p => p.CloseApplication("chrome"), Times.Once);
    }

    [Theory]
    [InlineData("115.0.5790.170", "115.0.5790.170")] // Clean version
    [InlineData(" 115.0.0.1 ", "115.0.0.1")] // Surrounding spaces
    [InlineData("115.0.0.1 (x64 de)", "115.0.0.1")] // Trailing info (Firefox style, but testing base class regex)
    [InlineData(null, "Unknown")] // Null case
    public void BrowserVersion_ShouldCleanAndReturnVersion(string? rawVersion, string expectedVersion)
    {
        // Arrange
        // RegistryKeyVersion for Chrome is @"Software\Google\Chrome\BLBeacon"
        _mockRegistryService.Setup(r => r.GetCurrentUserValue(@"Software\Google\Chrome\BLBeacon", "version")).Returns(rawVersion);

        // Act
        var result = _sut.BrowserVersion;

        // Assert
        result.ShouldBe(expectedVersion);
    }

    [Fact]
    public void IsExtensionInstalled_WhenNoProfileDirsFound_ShouldReturnFalse()
    {
        // Arrange
        _mockFileSystemService.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns(false);

        // Act
        var result = _sut.IsExtensionInstalled(); // Checks for Chrome ExtensionId

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsExtensionInstalled_WhenExtensionDirExists_ShouldReturnTrue()
    {
        // Arrange
        var root = @"C:\Users\Test\AppData\Local\Google\Chrome\User Data";
        _mockFileSystemService.Setup(f => f.GetEnvironmentPath("LocalAppData")).Returns(@"C:\Users\Test\AppData\Local");
        _mockFileSystemService.Setup(f => f.CombinePaths(It.IsAny<string[]>())).Returns<string[]>(paths => string.Join("\\", paths));
        
        // Mock that the Default profile exists
        _mockFileSystemService.Setup(f => f.DirectoryExists($@"{root}\Default")).Returns(true);
        
        // Chrome ExtensionId is "agchmcconfdfcenopioeilpgjngelefk"
        // Target path is Default\Extensions\agchmcconfdfcenopioeilpgjngelefk
        var extPath = $@"{root}\Default\Extensions\agchmcconfdfcenopioeilpgjngelefk";
        
        // Return true only for the target extension directory
        _mockFileSystemService.Setup(f => f.DirectoryExists(extPath)).Returns(true);

        // Act
        var result = _sut.IsExtensionInstalled();

        // Assert
        result.ShouldBeTrue();
    }
}