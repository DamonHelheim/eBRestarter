using NSubstitute;
using Shouldly;
using System.IO;
using Xunit;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Application;
using eBRestarter.Infrastructure.BehavioralComponents.Providers;

namespace eBRestarter.Tests.Infrastructure.Services.WindowsOS
{
    /// <summary>
    /// Unit tests for <see cref="WindowsAppPathProvider"/> verifying path combination and resolution.
    /// </summary>
    public class WindowsOsPathProviderAdapterTests
    {
        private readonly IOutboundPortAppPathProvider _mockPathProvider;
        private readonly WindowsAppPathProvider _sut;

        public WindowsOsPathProviderAdapterTests()
        {
            _mockPathProvider = Substitute.For<IOutboundPortAppPathProvider>();

            _sut = new WindowsAppPathProvider(_mockPathProvider);
        }

        [Fact]
        public void GetAppDataPath_ShouldCombineLocalAppData_WithSkylarAndAppFolder()
        {
            // [R]IGHT: Combines LocalAppData directory with company and application subfolder
            // Arrange
            string fakeLocalAppData = @"C:\FakeUsers\Max\AppData\Local";
            _mockPathProvider.RetrieveLocalAppDataDirectory().Returns(fakeLocalAppData);

            string expectedPath = Path.Combine(fakeLocalAppData, "Skylar", "eBRestarter");

            // Act
            string actualPath = _sut.RetrieveAppDataPath();

            // Assert
            actualPath.ShouldBe(expectedPath);
        }

        [Fact]
        public void GetDownloadsPath_ShouldCombineUserProfile_WithDownloadsFolder()
        {
            // [R]IGHT: Combines UserProfile directory with Downloads subfolder
            // Arrange
            string fakeUserProfile = @"C:\FakeUsers\Max";
            _mockPathProvider.RetrieveUserProfileDirectory().Returns(fakeUserProfile);

            string expectedPath = Path.Combine(fakeUserProfile, "Downloads");

            // Act
            string actualPath = _sut.RetrieveDownloadsPath();

            // Assert
            actualPath.ShouldBe(expectedPath);
        }

        [Fact]
        public void GetConfigFilePath_ShouldCombineAppDataPath_WithConfigFileName()
        {
            // [R]IGHT: Combines application data path with standard config JSON filename
            // Arrange
            string fakeLocalAppData = @"C:\FakeUsers\Max\AppData\Local";
            _mockPathProvider.RetrieveLocalAppDataDirectory().Returns(fakeLocalAppData);

            string expectedPath = Path.Combine(fakeLocalAppData, "Skylar", "eBRestarter", "eBRestarterConfig.json");

            // Act
            string actualPath = _sut.RetrieveConfigFilePath();

            // Assert
            actualPath.ShouldBe(expectedPath);
        }

        [Fact]
        public void GetLogFilePath_ShouldCombineAppDataPath_WithLogFileName()
        {
            // [R]IGHT: Combines application data path with standard log filename
            // Arrange
            string fakeLocalAppData = @"C:\FakeUsers\Max\AppData\Local";
            _mockPathProvider.RetrieveLocalAppDataDirectory().Returns(fakeLocalAppData);

            string expectedPath = Path.Combine(fakeLocalAppData, "Skylar", "eBRestarter", "log.txt");

            // Act
            string actualPath = _sut.RetrieveLogFilePath();

            // Assert
            actualPath.ShouldBe(expectedPath);
        }
    }
}
