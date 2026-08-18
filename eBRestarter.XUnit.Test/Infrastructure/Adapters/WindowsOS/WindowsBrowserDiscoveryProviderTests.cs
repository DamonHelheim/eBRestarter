using NSubstitute;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using eBRestarter.Core.Application.Common.Results;
using eBRestarter.Core.Application.ObjectArchetypes.Constants;
using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.ObjectArchetypes.Models;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;
using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Provider.WindowsOS;
using NSubstitute.ExceptionExtensions;

namespace eBRestarter.Tests.Infrastructure.Adapters.WindowsOS
{
    /// <summary>
    /// Unit tests for <see cref="AdapterWindowsBrowserDiscoveryProvider"/> verifying browser discovery, model mapping, and exception resilience.
    /// </summary>
    public class WindowsBrowserDiscoveryProviderTests
    {
        private readonly IOutboundPortBrowserFactory _mockBrowserFactory;

        public WindowsBrowserDiscoveryProviderTests()
        {
            _mockBrowserFactory = Substitute.For<IOutboundPortBrowserFactory>();
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenFactoryIsNull()
        {
            // [B]OUNDARY / [E]RROR: Null factory dependency throws ArgumentNullException
            // Arrange
            var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<AdapterWindowsBrowserDiscoveryProvider>.Instance;

            // Act
            Action act = () => new AdapterWindowsBrowserDiscoveryProvider(null!, logger);

            // Assert
            act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("browserFactory");
        }

        [Fact]
        public async Task GetInstalledBrowsersAsync_ShouldMapInstalledBrowserCorrectly()
        {
            // [R]IGHT: Maps installed browser properties correctly across all supported browser types
            // Arrange
            var service = new AdapterWindowsBrowserDiscoveryProvider(_mockBrowserFactory, Microsoft.Extensions.Logging.Abstractions.NullLogger<AdapterWindowsBrowserDiscoveryProvider>.Instance);
            int enumCount = Enum.GetValues<BrowserType>().Length;

            var mockInstalledBrowser = Substitute.For<IOutboundPortBrowser>();
            mockInstalledBrowser.IsInstalled.Returns(true);
            mockInstalledBrowser.BrowserVersion.Returns("120.0.6099.109");
            mockInstalledBrowser.DisplayName.Returns("Mocked Browser");
            mockInstalledBrowser.IconPath.Returns(@"C:\icon.exe");
            mockInstalledBrowser.DownloadUrl.Returns("https://download.com");

            _mockBrowserFactory
                .Create(Arg.Any<BrowserType>())
                .Returns(mockInstalledBrowser);

            // Act
            var result = await service.FindInstalledBrowsersAsync();

            // Assert
            var browserList = result.ToList();
            browserList.Count.ShouldBe(enumCount);

            var firstBrowser = browserList.First();
            firstBrowser.IsInstalled.ShouldBeTrue();
            firstBrowser.Version.ShouldBe("120.0.6099.109");
            firstBrowser.Name.ShouldBe("Mocked Browser");
            firstBrowser.IconPath.ShouldBe(@"C:\icon.exe");
            firstBrowser.DownloadUrl.ShouldBe("https://download.com");
        }

        [Fact]
        public async Task GetInstalledBrowsersAsync_ShouldMapUninstalledBrowserCorrectly()
        {
            // [B]OUNDARY: Uninstalled browser maps version fallback to 'Not installed'
            // Arrange
            var service = new AdapterWindowsBrowserDiscoveryProvider(_mockBrowserFactory, Microsoft.Extensions.Logging.Abstractions.NullLogger<AdapterWindowsBrowserDiscoveryProvider>.Instance);

            var mockUninstalledBrowser = Substitute.For<IOutboundPortBrowser>();
            mockUninstalledBrowser.IsInstalled.Returns(false);
            mockUninstalledBrowser.BrowserVersion.Returns("ShouldBeIgnored");
            mockUninstalledBrowser.DisplayName.Returns("Missing Browser");

            _mockBrowserFactory
                .Create(Arg.Any<BrowserType>())
                .Returns(mockUninstalledBrowser);

            // Act
            var result = await service.FindInstalledBrowsersAsync();

            // Assert
            var firstBrowser = result.First();
            firstBrowser.IsInstalled.ShouldBeFalse();
            firstBrowser.Version.ShouldBe("Not installed");
        }

        [Fact]
        public async Task GetInstalledBrowsersAsync_ShouldSkipBrowser_WhenFactoryThrowsNotSupportedException()
        {
            // [E]RROR: NotSupportedException from factory skips unsupported browser entry
            // Arrange
            var service = new AdapterWindowsBrowserDiscoveryProvider(_mockBrowserFactory, Microsoft.Extensions.Logging.Abstractions.NullLogger<AdapterWindowsBrowserDiscoveryProvider>.Instance);

            _mockBrowserFactory
                .Create(Arg.Any<BrowserType>())
                .Throws<NotSupportedException>();

            // Act
            var result = await service.FindInstalledBrowsersAsync();

            // Assert
            result.ShouldBeEmpty();
        }

        [Fact]
        public async Task GetInstalledBrowsersAsync_ShouldHandleGeneralExceptions_Gracefully()
        {
            // [E]RROR: Property access exception is caught gracefully and returns empty list
            // Arrange
            var service = new AdapterWindowsBrowserDiscoveryProvider(_mockBrowserFactory, Microsoft.Extensions.Logging.Abstractions.NullLogger<AdapterWindowsBrowserDiscoveryProvider>.Instance);

            var buggyBrowserMock = Substitute.For<IOutboundPortBrowser>();
            buggyBrowserMock.IsInstalled.Throws(new InvalidOperationException("Registry Error"));

            _mockBrowserFactory
                .Create(Arg.Any<BrowserType>())
                .Returns(buggyBrowserMock);

            // Act
            var result = await service.FindInstalledBrowsersAsync();

            // Assert
            result.ShouldBeEmpty();
        }
    }
}









