using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Application.UseCases.DeleteBrowserContent;
using eBRestarter.Core.Domain.Enums;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace eBRestarter.XUnit.Test.Application.UseCases;

public class DeleteBrowserContentServiceTests
{
    private readonly Mock<IBrowserFactory> _mockBrowserFactory;
    private readonly Mock<IFileDeletionService> _mockFileDeletionService;
    private readonly Mock<IWindowsProcessControlService> _mockProcessService;
    private readonly Mock<ILocalizationService> _mockLocalizationService;
    private readonly Mock<IBrowser> _mockBrowser;
    private readonly DeleteBrowserContentService _sut;

    public DeleteBrowserContentServiceTests()
    {
        _mockBrowserFactory = new Mock<IBrowserFactory>();
        _mockFileDeletionService = new Mock<IFileDeletionService>();
        _mockProcessService = new Mock<IWindowsProcessControlService>();
        _mockLocalizationService = new Mock<ILocalizationService>();
        _mockBrowser = new Mock<IBrowser>();

        _mockBrowserFactory.Setup(f => f.Create(It.IsAny<BrowserType>())).Returns(_mockBrowser.Object);
        _mockLocalizationService.Setup(l => l.GetString(It.IsAny<string>())).Returns((string key) => key);

        _sut = new DeleteBrowserContentService(
            _mockBrowserFactory.Object,
            _mockFileDeletionService.Object,
            _mockProcessService.Object,
            _mockLocalizationService.Object);
    }

        [Fact]
    public async Task ExecuteAsync_WhenBrowserIsRunning_ShouldReturnConflictResponse()
    {
        // Arrange
        var request = new DeleteBrowserContentRequest(BrowserType.Chrome, true, true, false);
        _mockProcessService.Setup(p => p.IsProcessAlive("chrome")).Returns(true);
        var progress = new Progress<DeleteBrowserContentProgress>();

        // Act
        var result = await _sut.ExecuteAsync(request, progress, CancellationToken.None);

        // Assert
        result.Success.ShouldBeFalse();
        result.ProcessConflict.ShouldBeTrue();
        result.ErrorMessage.ShouldBe("Cleanup_BrowserRunning");
        _mockFileDeletionService.Verify(f => f.DeleteFilesAsync(It.IsAny<List<string>>(), It.IsAny<IProgress<string>>(), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenForceClose_ShouldCloseProcessAndContinue()
    {
        // Arrange
        var request = new DeleteBrowserContentRequest(BrowserType.Chrome, true, true, true);
        _mockProcessService.Setup(p => p.IsProcessAlive("chrome")).Returns(false); // Closed successfully

        _mockBrowser.Setup(b => b.GetPaths()).Returns(new Core.Domain.Models.Records.BrowserPaths(
            new List<string> { "cacheDir1" },
            new List<string> { "cookieDir1" },
            new List<string>()
        ));

        _mockFileDeletionService.Setup(f => f.CountFilesAsync(It.IsAny<List<string>>())).ReturnsAsync(10);
        
        var progress = new Progress<DeleteBrowserContentProgress>();

        // Act
        var result = await _sut.ExecuteAsync(request, progress, CancellationToken.None);

        // Assert
        _mockProcessService.Verify(p => p.CloseApplication("chrome"), Times.Once);
        result.Success.ShouldBeTrue();
        result.ProcessConflict.ShouldBeFalse();
        _mockFileDeletionService.Verify(f => f.DeleteFilesAsync(It.IsAny<List<string>>(), It.IsAny<IProgress<string>>(), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenExceptionOccurs_ShouldReturnErrorResponse()
    {
        // Arrange
        var request = new DeleteBrowserContentRequest(BrowserType.Edge, true, true, false);
        _mockProcessService.Setup(p => p.IsProcessAlive("msedge")).Returns(false);
        
        _mockBrowser.Setup(b => b.GetPaths()).Returns(new Core.Domain.Models.Records.BrowserPaths(
            new List<string> { "cacheDir1" },
            new List<string>(),
            new List<string>()
        ));

        _mockFileDeletionService.Setup(f => f.CountFilesAsync(It.IsAny<List<string>>())).ThrowsAsync(new Exception("Test IO Error"));
        var progress = new Progress<DeleteBrowserContentProgress>();

        // Act
        var result = await _sut.ExecuteAsync(request, progress, CancellationToken.None);

        // Assert
        result.Success.ShouldBeFalse();
        result.ErrorMessage.ShouldBe("Test IO Error");
    }
}
