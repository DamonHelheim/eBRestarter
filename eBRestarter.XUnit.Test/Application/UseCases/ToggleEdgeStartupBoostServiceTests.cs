using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Application.UseCases.ToggleEdgeStartupBoost;
using eBRestarter.Core.Domain.Enums;
using Moq;
using Shouldly;
using System;
using Xunit;

namespace eBRestarter.XUnit.Test.Application.UseCases;

public class ToggleEdgeStartupBoostServiceTests
{
    private readonly Mock<IWindowsStartupManagerService> _mockStartupService;
    private readonly Mock<IBrowserFactory> _mockBrowserFactory;
    private readonly Mock<IBrowser> _mockBrowser;
    private readonly ToggleEdgeStartupBoostService _sut;

    public ToggleEdgeStartupBoostServiceTests()
    {
        _mockStartupService = new Mock<IWindowsStartupManagerService>();
        _mockBrowserFactory = new Mock<IBrowserFactory>();
        _mockBrowser = new Mock<IBrowser>();

        _sut = new ToggleEdgeStartupBoostService(_mockStartupService.Object, _mockBrowserFactory.Object);
    }

    [Fact]
    public void IsEnabled_ShouldReturnServiceStatus()
    {
        // Arrange
        _mockStartupService.Setup(s => s.IsEdgeStartupBoostEnabled()).Returns(true);

        // Act
        var result = _sut.IsEnabled();

        // Assert
        result.ShouldBeTrue();
        _mockStartupService.Verify(s => s.IsEdgeStartupBoostEnabled(), Times.Once);
    }

    [Fact]
    public void IsEdgeInstalled_WhenInstalled_ShouldReturnTrue()
    {
        // Arrange
        _mockBrowser.Setup(b => b.IsInstalled).Returns(true);
        _mockBrowserFactory.Setup(f => f.Create(BrowserType.Edge)).Returns(_mockBrowser.Object);

        // Act
        var result = _sut.IsEdgeInstalled();

        // Assert
        result.ShouldBeTrue();
        _mockBrowserFactory.Verify(f => f.Create(BrowserType.Edge), Times.Once);
    }

    [Fact]
    public void Toggle_WhenSuccessful_ShouldReturnSuccessResponse()
    {
        // Arrange
        var enable = true;

        // Act
        var result = _sut.Toggle(enable);

        // Assert
        result.Success.ShouldBeTrue();
        result.NewState.ShouldBe(enable);
        result.ErrorMessage.ShouldBeEmpty();
        _mockStartupService.Verify(s => s.SetEdgeStartupBoost(enable), Times.Once);
    }

    [Fact]
    public void Toggle_WhenExceptionThrown_ShouldReturnErrorResponse()
    {
        // Arrange
        var enable = false;
        var exceptionMessage = "Access Denied";
        _mockStartupService.Setup(s => s.SetEdgeStartupBoost(It.IsAny<bool>())).Throws(new UnauthorizedAccessException(exceptionMessage));

        // Act
        var result = _sut.Toggle(enable);

        // Assert
        result.Success.ShouldBeFalse();
        result.NewState.ShouldBe(!enable); // Should return the opposite state
        result.ErrorMessage.ShouldBe(exceptionMessage);
    }
}
