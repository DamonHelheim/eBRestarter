using eBRestarter.Core.Domain.Enums;
using eBRestarter.Infrastructure.Browsers;
using eBRestarter.Infrastructure.Factories;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using System;
using Xunit;
using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using Microsoft.Extensions.Logging;

namespace eBRestarter.XUnit.Test.Infrastructure.Factories;

public class BrowserFactoryTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly BrowserFactory _sut;

    public BrowserFactoryTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _sut = new BrowserFactory(_mockServiceProvider.Object);
    }

    [Fact]
    public void Create_WhenChromeRequested_ShouldResolveChromeBrowser()
    {
        // Arrange
        var mockOs = new Mock<IOperatingSystemFacade>();
        var mockLogger = new Mock<ILogger<ChromeBrowser>>();
        var chrome = new ChromeBrowser(mockOs.Object, mockLogger.Object);
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(ChromeBrowser))).Returns(chrome);

        // Act
        var result = _sut.Create(BrowserType.Chrome);

        // Assert
        result.ShouldBeOfType<ChromeBrowser>();
        result.Type.ShouldBe(BrowserType.Chrome);
    }

    [Fact]
    public void Create_WhenFirefoxRequested_ShouldResolveFirefoxBrowser()
    {
        // Arrange
        var mockOs = new Mock<IOperatingSystemFacade>();
        var mockLogger = new Mock<ILogger<FirefoxBrowser>>();
        var firefox = new FirefoxBrowser(mockOs.Object, mockLogger.Object);
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(FirefoxBrowser))).Returns(firefox);

        // Act
        var result = _sut.Create(BrowserType.Firefox);

        // Assert
        result.ShouldBeOfType<FirefoxBrowser>();
        result.Type.ShouldBe(BrowserType.Firefox);
    }

    [Fact]
    public void Create_WhenUnsupportedRequested_ShouldThrowException()
    {
        // Arrange
        // (BrowserType)99 is an invalid enum value, simulating unsupported browser
        var unsupportedType = (BrowserType)99;

        // Act & Assert
        var exception = Should.Throw<NotSupportedException>(() => _sut.Create(unsupportedType));
        exception.Message.ShouldContain("ist noch nicht implementiert");
    }
}