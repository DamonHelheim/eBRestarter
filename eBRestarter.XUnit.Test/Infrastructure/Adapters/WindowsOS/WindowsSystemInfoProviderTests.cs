using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using System;
using Xunit;
using eBRestarter.Core.Application.Common.Results;
using eBRestarter.Core.Application.ObjectArchetypes.Constants;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Provider.WindowsOS;
using NSubstitute.ExceptionExtensions;
using Microsoft.Extensions.Logging.Testing;

namespace eBRestarter.Tests.Infrastructure.Adapters.WindowsOS
{
    /// <summary>
    /// Unit tests for <see cref="AdapterWindowsSystemInfoProvider"/> verifying OS display version, build number extraction, and standard browser detection.
    /// </summary>
    public class WindowsSystemInfoProviderTests
    {
        private readonly FakeLogger<AdapterWindowsSystemInfoProvider> _mockLogger;
        private readonly IOutboundPortSystemConfigurationRepository _mockRegistry;
        private readonly AdapterWindowsSystemInfoProvider _sut;

        private const string RegistryPathCurrentVersion = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
        private const string RegistryPathUserChoiceHttp = @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice";
        private const string RegistryPathUserChoiceHttps = @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\https\UserChoice";

        public WindowsSystemInfoProviderTests()
        {
            _mockLogger = new FakeLogger<AdapterWindowsSystemInfoProvider>();
            _mockRegistry = Substitute.For<IOutboundPortSystemConfigurationRepository>();

            _sut = new AdapterWindowsSystemInfoProvider(_mockLogger, _mockRegistry);
        }

        [Fact]
        public void GetCurrentOsDisplayVersion_ShouldReturnVersion_WhenRegistryKeyExists()
        {
            // [R]IGHT: Returns OS display version string when registry key is present
            // Arrange
            _mockRegistry.GetSystemValue(RegistryPathCurrentVersion, "DisplayVersion")
                         .Returns("22H2");

            // Act
            var result = _sut.RetrieveCurrentOsDisplayVersion();

            // Assert
            result.ShouldBe("22H2");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void GetCurrentOsDisplayVersion_ShouldReturnUnknown_WhenKeyIsMissingOrEmpty(object? invalidValue)
        {
            // [B]OUNDARY: Returns 'Unknown' when DisplayVersion registry key is missing, null, or whitespace
            // Arrange
            _mockRegistry.GetSystemValue(RegistryPathCurrentVersion, "DisplayVersion")
                         .Returns(invalidValue);

            // Act
            var result = _sut.RetrieveCurrentOsDisplayVersion();

            // Assert
            result.ShouldBe("Unknown");
        }

        [Fact]
        public void GetCurrentOsDisplayVersion_ShouldReturnError_OnException()
        {
            // [E]RROR: UnauthorizedAccessException returns 'Error' fallback
            // Arrange
            _mockRegistry.GetSystemValue(Arg.Any<string>(), Arg.Any<string>())
                         .Throws(new UnauthorizedAccessException("No rights"));

            // Act
            var result = _sut.RetrieveCurrentOsDisplayVersion();

            // Assert
            result.ShouldBe("Error");
        }

        [Fact]
        public void GetCurrentOsBuildVersion_ShouldCombineEnvironmentBuild_WithUbrFromRegistry()
        {
            // [R]IGHT: Combines environment build number with UBR revision from registry
            // Arrange
            _mockRegistry.GetSystemValue(RegistryPathCurrentVersion, "UBR")
                         .Returns(1234); // UBR is stored as a DWORD (int) in the registry

            string expectedBuild = $"{Environment.OSVersion.Version.Build}.1234";

            // Act
            var result = _sut.RetrieveCurrentOsBuildVersion();

            // Assert
            result.ShouldBe(expectedBuild);
        }

        [Fact]
        public void GetCurrentOsBuildVersion_ShouldDefaultToZero_WhenUbrIsMissing()
        {
            // [B]OUNDARY: Defaults revision to '.0' when UBR registry value is missing
            // Arrange
            _mockRegistry.GetSystemValue(RegistryPathCurrentVersion, "UBR")
                         .Returns(null);

            string expectedBuild = $"{Environment.OSVersion.Version.Build}.0";

            // Act
            var result = _sut.RetrieveCurrentOsBuildVersion();

            // Assert
            result.ShouldBe(expectedBuild);
        }

        [Fact]
        public void GetCurrentOsBuildVersion_ShouldReturnError_OnException()
        {
            // [E]RROR: Exception during registry read returns 'Error' fallback
            // Arrange
            _mockRegistry.GetSystemValue(Arg.Any<string>(), Arg.Any<string>())
                         .Throws(new Exception("Registry error"));

            // Act
            var result = _sut.RetrieveCurrentOsBuildVersion();

            // Assert
            result.ShouldBe("Error");
        }

        [Theory]
        [InlineData("ChromeHTML", "Chrome")]
        [InlineData("FirefoxURL-308046B0AF4A39CB", "Firefox")] // Firefox ProgId includes hash suffix
        [InlineData("MSEdgeHTM", "Edge")]
        [InlineData("OperaStable", "Opera")]
        [InlineData("BraveHTML", "Brave")]
        [InlineData("UnbekannterBrowserXYZ", "-")] // Unknown ProgId resolves to dash fallback
        public void GetCurrentStandardBrowserName_ShouldMapProgIdCorrectly(string progId, string expectedBrowserName)
        {
            // [R]IGHT / [B]OUNDARY: Maps registry ProgId to friendly browser name across known and unknown identifiers
            // Arrange
            _mockRegistry.GetUserValue(RegistryPathUserChoiceHttp, "ProgId").Returns(progId);
            _mockRegistry.GetUserValue(RegistryPathUserChoiceHttps, "ProgId").Returns(progId);

            // Act
            var result = _sut.RetrieveCurrentStandardBrowserName();

            // Assert
            result.ShouldBe(expectedBrowserName);
        }

        [Fact]
        public void GetCurrentStandardBrowserName_ShouldHandleMismatch_BetweenHttpAndHttps()
        {
            // [B]OUNDARY: Resolves browser from HTTP association when HTTP and HTTPS ProgIds mismatch
            // Arrange
            _mockRegistry.GetUserValue(RegistryPathUserChoiceHttp, "ProgId").Returns("ChromeHTML");
            _mockRegistry.GetUserValue(RegistryPathUserChoiceHttps, "ProgId").Returns("FirefoxURL");

            // Act
            var result = _sut.RetrieveCurrentStandardBrowserName();

            // Assert
            result.ShouldBe("Chrome");
        }

        [Theory]
        [InlineData(null, "ChromeHTML")]
        [InlineData("ChromeHTML", "")]
        [InlineData(null, null)]
        public void GetCurrentStandardBrowserName_ShouldReturnDash_WhenAnyProgIdIsMissing(string? httpProgId, string? httpsProgId)
        {
            // [B]OUNDARY: Returns '-' when either HTTP or HTTPS ProgId is missing or empty
            // Arrange
            _mockRegistry.GetUserValue(RegistryPathUserChoiceHttp, "ProgId").Returns(httpProgId);
            _mockRegistry.GetUserValue(RegistryPathUserChoiceHttps, "ProgId").Returns(httpsProgId);

            // Act
            var result = _sut.RetrieveCurrentStandardBrowserName();

            // Assert
            result.ShouldBe("-");
        }

        [Fact]
        public void GetCurrentStandardBrowserName_ShouldReturnDash_OnException()
        {
            // [E]RROR: UnauthorizedAccessException during registry read returns '-' fallback
            // Arrange
            _mockRegistry.GetUserValue(Arg.Any<string>(), Arg.Any<string>())
                         .Throws(new UnauthorizedAccessException("No rights for HKCU"));

            // Act
            var result = _sut.RetrieveCurrentStandardBrowserName();

            // Assert
            result.ShouldBe("-");
        }
    }
}
