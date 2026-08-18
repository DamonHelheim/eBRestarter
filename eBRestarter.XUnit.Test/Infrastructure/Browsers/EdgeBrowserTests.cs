using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Wrapper.Browsers;
using Microsoft.Extensions.Logging.Testing;

namespace eBRestarter.XUnit.Test.Infrastructure.Browsers
{
    /// <summary>
    /// Unit tests for <see cref="AdapterEdgeBrowserWrapper"/> verifying default profile cache and network path resolution.
    /// </summary>
    public class EdgeBrowserTests
    {
        [Fact]
        public void GetPaths_ShouldGenerateCorrectDirectories_ForEdge()
        {
            // [R]IGHT: Generates expected cache and cookie directory paths for default Edge profile
            // Arrange
            var mockProcess = Substitute.For<IOutboundPortOsProcessControl>();
            var mockSettings = Substitute.For<IOutboundPortSystemConfigurationRepository>();
            var mockFileSystem = Substitute.For<IOutboundPortFileSystem>();
            var mockLogger = new FakeLogger<AdapterEdgeBrowserWrapper>();

            mockFileSystem.ResolveEnvironmentPath("LocalAppData").Returns(@"C:\Local");
            mockFileSystem.CombinePaths(Arg.Any<string[]>()).Returns(ci => string.Join(@"\", ci.Arg<string[]>()));

            string expectedDefaultProfilePath = @"C:\Local\Microsoft\Edge\User Data\Default";
            mockFileSystem.DirectoryExists(expectedDefaultProfilePath).Returns(true);

            var edgeBrowser = new AdapterEdgeBrowserWrapper(mockProcess, mockSettings, mockFileSystem, mockLogger);

            // Act
            var paths = edgeBrowser.ResolvePaths();

            // Assert
            paths.CacheDirs.ShouldContain($@"{expectedDefaultProfilePath}\GPUCache");
            paths.CookiesDirs.ShouldContain($@"{expectedDefaultProfilePath}\Network");
        }
    }
}
