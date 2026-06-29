using eBRestarter.Core.Application.Ports.Outbound;
using eBRestarter.Core.Application.Ports.Outbound.Providers;
using eBRestarter.Core.Application.Providers;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Core.Application.Ports.Outbound.Browser;
using eBRestarter.Core.Application.Validators;
using eBRestarter.Core.Application.Ports.Inbound.Providers;
using eBRestarter.Core.Application.Ports.Outbound.Formatters;
using eBRestarter.Core.Application.Ports.Outbound.Application;

using eBRestarter.Core.Application.Ports.Outbound.Network;
using eBRestarter.Core.Application.Ports.Outbound.Network;
using eBRestarter.Core.Application.Ports.Inbound.UseCases.DeleteBrowserContent;
using eBRestarter.Core.Application.UseCases.DeleteBrowserContent;
using eBRestarter.Core.Application.Models.Errors;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Records;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace eBRestarter.Tests.Core.Application.UseCases.DeleteBrowserContent
{
    public class DeleteBrowserContentUseCaseTests
    {
        private readonly Mock<IBrowserFactoryPort> _mockBrowserFactory;
        private readonly Mock<IFileDeletionPort> _mockFileDeletionService;
        private readonly Mock<IOsProcessControlPort> _mockProcessService;
        private readonly Mock<ILocalizationProvider> _mockLocalizationService;
        private readonly DeleteBrowserContentUseCase _sut;

        public DeleteBrowserContentUseCaseTests()
        {
            _mockBrowserFactory = new Mock<IBrowserFactoryPort>();
            _mockFileDeletionService = new Mock<IFileDeletionPort>();
            _mockProcessService = new Mock<IOsProcessControlPort>();
            _mockLocalizationService = new Mock<ILocalizationProvider>();

            _mockLocalizationService
                .Setup(l => l.RetrieveString(It.IsAny<string>()))
                .Returns((string key) => key);

            _sut = new DeleteBrowserContentUseCase(
                _mockBrowserFactory.Object,
                _mockFileDeletionService.Object,
                _mockProcessService.Object,
                _mockLocalizationService.Object,
                new DeleteBrowserContentValidator());
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnProcessConflict_WhenBrowserIsStillRunning()
        {
            var request = new DeleteBrowserContentRequest(BrowserType.Chrome, true, true, ForceCloseProcess: false);
            var progress = new Progress<DeleteBrowserContentProgress>();

            var mockBrowser = new Mock<IBrowserPort>();
            mockBrowser.Setup(b => b.ProcessName).Returns("chrome");
            _mockBrowserFactory.Setup(f => f.Create(BrowserType.Chrome)).Returns(mockBrowser.Object);

            _mockProcessService.Setup(p => p.IsProcessAlive("chrome")).Returns(true);

            var result = await _sut.ExecuteAsync(request, progress, CancellationToken.None);

            result.IsSuccess.ShouldBeFalse();
            result.HasError<ProcessConflictError>().ShouldBeTrue();

            _mockFileDeletionService.Verify(f => f.DeleteFilesAsync(It.IsAny<List<string>>(), It.IsAny<IProgress<string>>(), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldForceCloseProcess_WhenRequested()
        {
            var request = new DeleteBrowserContentRequest(BrowserType.Edge, true, true, ForceCloseProcess: true);
            var progressUpdates = new List<DeleteBrowserContentProgress>();
            var progress = new Progress<DeleteBrowserContentProgress>(p => progressUpdates.Add(p));

            var mockBrowser = new Mock<IBrowserPort>();
            mockBrowser.Setup(b => b.ProcessName).Returns("msedge");

            mockBrowser.Setup(b => b.ResolvePaths()).Returns(new BrowserPaths(
                new List<string> { "C:\\Cache" },
                new List<string>(),
                new List<string>()));

            _mockBrowserFactory.Setup(f => f.Create(BrowserType.Edge)).Returns(mockBrowser.Object);

            _mockProcessService.Setup(p => p.IsProcessAlive("msedge")).Returns(false);

            var result = await _sut.ExecuteAsync(request, progress, CancellationToken.None);

            _mockProcessService.Verify(p => p.CloseApplication("msedge"), Times.Once);

            progressUpdates.ShouldContain(p => p.StatusMessage == "Cleanup_ClosingBrowser");
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnNoPathsError_WhenNothingIsSelectedOrAvailable()
        {
            var request = new DeleteBrowserContentRequest(BrowserType.Firefox, DeleteCookies: false, DeleteCache: false, ForceCloseProcess: false);
            var progress = new Progress<DeleteBrowserContentProgress>();

            var mockBrowser = new Mock<IBrowserPort>();
            mockBrowser.Setup(b => b.ProcessName).Returns("firefox");

            mockBrowser.Setup(b => b.ResolvePaths()).Returns(new BrowserPaths(
                new List<string> { "C:\\Cache" },
                new List<string> { "C:\\Cookies" },
                new List<string>()));

            _mockBrowserFactory.Setup(f => f.Create(BrowserType.Firefox)).Returns(mockBrowser.Object);

            _mockProcessService.Setup(p => p.IsProcessAlive("firefox")).Returns(false);

            var result = await _sut.ExecuteAsync(request, progress, CancellationToken.None);

            result.IsSuccess.ShouldBeFalse();
            result.Errors[0].Message.ShouldBe("At least one deletion option (Cache or Cookies) must be selected.");
        }

        [Fact]
        public async Task ExecuteAsync_ShouldCallDeletionService_WithCombinedPaths()
        {
            var request = new DeleteBrowserContentRequest(BrowserType.Brave, DeleteCookies: true, DeleteCache: true, ForceCloseProcess: false);
            var progressUpdates = new List<DeleteBrowserContentProgress>();
            var progress = new Progress<DeleteBrowserContentProgress>(p => progressUpdates.Add(p));

            var mockBrowser = new Mock<IBrowserPort>();
            mockBrowser.Setup(b => b.ProcessName).Returns("brave");

            mockBrowser.Setup(b => b.ResolvePaths()).Returns(new BrowserPaths(
                new List<string> { "C:\\Brave\\Cache" },
                new List<string> { "C:\\Brave\\Cookies" },
                new List<string>()));

            _mockBrowserFactory.Setup(f => f.Create(BrowserType.Brave)).Returns(mockBrowser.Object);

            _mockProcessService.Setup(p => p.IsProcessAlive("brave")).Returns(false);

            _mockFileDeletionService.Setup(f => f.CountFilesAsync(It.IsAny<List<string>>())).ReturnsAsync(100);

            var result = await _sut.ExecuteAsync(request, progress, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();

            _mockFileDeletionService.Verify(f => f.DeleteFilesAsync(
                It.Is<List<string>>(list => list.Contains("C:\\Brave\\Cache") && list.Contains("C:\\Brave\\Cookies") && list.Count == 2),
                It.IsAny<IProgress<string>>(),
                It.IsAny<IProgress<int>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

            progressUpdates.ShouldContain(p => p.StatusMessage == "Cleanup_Finished" && p.TotalFiles == 100 && p.CurrentFile == 100);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldCatchExceptions_AndReturnFailure()
        {
            var request = new DeleteBrowserContentRequest(BrowserType.Vivaldi, true, true, false);
            var progress = new Progress<DeleteBrowserContentProgress>();

            _mockBrowserFactory.Setup(f => f.Create(It.IsAny<BrowserType>())).Throws(new UnauthorizedAccessException("Zugriff verweigert"));

            var result = await _sut.ExecuteAsync(request, progress, CancellationToken.None);

            result.IsSuccess.ShouldBeFalse();
            result.Errors[0].Message.ShouldBe("Zugriff verweigert");
        }
    }
}














