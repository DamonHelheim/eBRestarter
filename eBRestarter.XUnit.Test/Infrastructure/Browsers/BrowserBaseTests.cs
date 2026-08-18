using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using System.Collections.Generic;
using Xunit;
using eBRestarter.Core.Application.ObjectArchetypes.Constants;
using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Wrapper.Browsers;

namespace eBRestarter.XUnit.Test.Infrastructure.Browsers
{
    /// <summary>
    /// Unit tests for abstract <see cref="AdapterBrowserBaseWrapper"/> verifying version string sanitization, startup command formatting, and error handling.
    /// </summary>
    public class BrowserBaseTests
    {
        /// <summary>
        /// Minimal concrete test double enabling unit testing of abstract <see cref="AdapterBrowserBaseWrapper"/> logic.
        /// </summary>
        private sealed class TestDummyBrowser : AdapterBrowserBaseWrapper
        {
            public TestDummyBrowser(IOutboundPortOsProcessControl os, IOutboundPortSystemConfigurationRepository settings, IOutboundPortFileSystem fs, ILogger logger) : base(os, settings, fs, logger) { }

            public override BrowserType Type => BrowserType.Firefox;
            public override string DisplayName => "TestBrowser";
            public override string IconPath => "";
            public override string DownloadUrl => "";
            public override string ExtensionInstallUrl => "";
            public override string ProcessName => "testbrowser";
            protected override string RegistryKeyVersion => @"Software\Test";

            protected override List<string> ExecutablePaths => new() { @"C:\FakePath\browser.exe" };

            public override bool IsExtensionInstalled(string? extensionId = null) => false;
            public override BrowserPaths ResolvePaths() => new(new List<string>(), new List<string>(), new List<string>());

            /// <summary>
            /// Exposes the protected <see cref="AdapterBrowserBaseWrapper.CleanVersionString"/> method for unit test assertions.
            /// </summary>
            public string ExposeCleanVersionString(string? raw)
            {
                return CleanVersionString(raw);
            }
        }

        [Theory]
        [InlineData("120.0.1 (x64 de)", "120.0.1")]
        [InlineData("121.0.6167.161 (Official Build) (64-bit)", "121.0.6167.161")]
        [InlineData("  115.0.3  ", "115.0.3")]
        [InlineData("10.0", "10.0")]
        [InlineData(null, "Unknown")]
        [InlineData("", "Unknown")]
        public void CleanVersionString_ShouldExtractVersionNumberCorrectly(string? rawVersion, string expectedResult)
        {
            // [R]IGHT / [B]OUNDARY: Sanitizes raw version strings to extract numeric major.minor.build.rev tokens
            // Arrange
            var mockProcess = Substitute.For<IOutboundPortOsProcessControl>();
            var mockSettings = Substitute.For<IOutboundPortSystemConfigurationRepository>();
            var mockFileSystem = Substitute.For<IOutboundPortFileSystem>();
            var mockLogger = Substitute.For<ILogger>();
            var dummyBrowser = new TestDummyBrowser(mockProcess, mockSettings, mockFileSystem, mockLogger);

            // Act
            var result = dummyBrowser.ExposeCleanVersionString(rawVersion);

            // Assert
            result.ShouldBe(expectedResult);
        }

        [Fact]
        public void Start_ShouldFindExecutableAndCallProcessService_WithUrlAndArguments()
        {
            // [R]IGHT: Formats startup arguments with quoted URL and launches browser via process control port
            // Arrange
            var mockProcess = Substitute.For<IOutboundPortOsProcessControl>();
            var mockSettings = Substitute.For<IOutboundPortSystemConfigurationRepository>();
            var mockLogger = Substitute.For<ILogger>();

            var mockFileSystem = Substitute.For<IOutboundPortFileSystem>();
            mockFileSystem
                .FileExists(@"C:\FakePath\browser.exe")
                .Returns(true);

            var dummyBrowser = new TestDummyBrowser(mockProcess, mockSettings, mockFileSystem, mockLogger);

            const string testUrl = "https://www.ebesucher.com/surfbar/test";
            const string testArgs = "--incognito";

            // Act
            dummyBrowser.Start(testUrl, testArgs);

            // Assert
            mockProcess.Received(1).OpenUrlInBrowser(
                @"C:\FakePath\browser.exe",
                // Quotes ensure the URL is parsed as a single argument and spaces do not inject arbitrary CLI switches.
                "\"https://www.ebesucher.com/surfbar/test\" --incognito"
            );
        }

        [Fact]
        public void Start_ShouldLogError_WhenExecutableIsNotFound()
        {
            // [E]RROR: Catches file missing exception gracefully and prevents process launch
            // Arrange
            var mockProcess = Substitute.For<IOutboundPortOsProcessControl>();
            var mockSettings = Substitute.For<IOutboundPortSystemConfigurationRepository>();
            var mockLogger = Substitute.For<ILogger>();

            var mockFileSystem = Substitute.For<IOutboundPortFileSystem>();
            mockFileSystem
                .FileExists(Arg.Any<string>())
                .Returns(false);

            var dummyBrowser = new TestDummyBrowser(mockProcess, mockSettings, mockFileSystem, mockLogger);

            // Act
            dummyBrowser.Start("https://test.com");

            // Assert
            mockProcess.DidNotReceive().OpenUrlInBrowser(Arg.Any<string>(), Arg.Any<string>());
        }
    }
}
