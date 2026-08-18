using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;
using eBRestarter.Core.Application.ObjectArchetypes.Constants;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Wrapper.Browsers;
using Microsoft.Extensions.Logging.Testing;

namespace eBRestarter.XUnit.Test.Infrastructure.Browsers
{
    /// <summary>
    /// Unit tests for <see cref="AdapterBraveBrowserWrapper"/> verifying profile and cache directory path resolution.
    /// </summary>
    public class BraveBrowserTests
    {
        [Fact]
        public void GetPaths_ShouldGenerateCorrectDirectories_ForBrave()
        {
            // [R]IGHT: Generates expected cache and cookie directory paths for default Brave profile
            // Arrange
            var mockProcess = Substitute.For<IOutboundPortOsProcessControl>();
            var mockSettings = Substitute.For<IOutboundPortSystemConfigurationRepository>();
            var mockFileSystem = Substitute.For<IOutboundPortFileSystem>();
            var mockLogger = new FakeLogger<AdapterBraveBrowserWrapper>();

            mockFileSystem.ResolveEnvironmentPath("LocalAppData").Returns(@"C:\Local");
            mockFileSystem.CombinePaths(Arg.Any<string[]>()).Returns(ci => string.Join(@"\", ci.Arg<string[]>()));

            string expectedDefaultProfilePath = @"C:\Local\BraveSoftware\Brave-Browser\User Data\Default";
            mockFileSystem.DirectoryExists(expectedDefaultProfilePath).Returns(true);

            var braveBrowser = new AdapterBraveBrowserWrapper(mockProcess, mockSettings, mockFileSystem, mockLogger);

            // Act
            var paths = braveBrowser.ResolvePaths();

            // Assert
            paths.CacheDirs.ShouldContain($@"{expectedDefaultProfilePath}\Cache\Cache_Data");
            paths.CookiesDirs.ShouldContain($@"{expectedDefaultProfilePath}\Local Storage");
        }
    }
}
