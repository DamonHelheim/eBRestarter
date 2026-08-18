using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Repositories.WindowsOS;
using NSubstitute.ExceptionExtensions;
using Microsoft.Extensions.Logging.Testing;

namespace eBRestarter.Tests.Infrastructure.Services.WindowsOS
{
    /// <summary>
    /// Unit tests for <see cref="AdapterWindowsStartupRepository"/> verifying autostart registration, Edge startup boost policies, and autologon configuration via mocked registry interactions.
    /// </summary>
    public class WindowsStartupRepositoryTests
    {
        private readonly FakeLogger<AdapterWindowsStartupRepository> _mockLogger;
        private readonly IOutboundPortSystemConfigurationRepository _mockRegistry;
        private readonly IOutboundPortProcessInfoProvider _mockProcessInfo;
        private readonly AdapterWindowsStartupRepository _sut;

        private const string RegistryPathRun = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string RegistryPathEdgePolicies = @"SOFTWARE\Policies\Microsoft\Edge";
        private const string RegistryPathPasswordLess = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\PasswordLess\Device";

        public WindowsStartupRepositoryTests()
        {
            _mockLogger = new FakeLogger<AdapterWindowsStartupRepository>();
            _mockRegistry = Substitute.For<IOutboundPortSystemConfigurationRepository>();
            _mockProcessInfo = Substitute.For<IOutboundPortProcessInfoProvider>();

            _sut = new AdapterWindowsStartupRepository(_mockLogger, _mockProcessInfo, _mockRegistry);
        }

        [Fact]
        public void EnableAutoStart_ShouldSetRegistryKey_WithCurrentExePath()
        {
            // [R]IGHT: Writes quoted executable path to HKCU Run key to enable automatic startup
            // Arrange
            string fakeExePath = @"C:\TestApp\eBRestarter.exe";
            _mockProcessInfo.GetCurrentExecutablePath().Returns(fakeExePath);

            // Act
            _sut.EnableAutoStart();

            // Assert
            // Quoting prevents path hijacking when paths contain whitespace characters
            _mockRegistry.Received(1).SetUserValue(
                RegistryPathRun,
                "eBRestarter",
                $"\"{fakeExePath}\"");
        }

        [Fact]
        public void DisableAutoStart_ShouldDeleteRegistryKey()
        {
            // [R]IGHT: Deletes autostart value from HKCU Run key
            // Act
            _sut.DisableAutoStart();

            // Assert
            _mockRegistry.Received(1).DeleteUserValue(RegistryPathRun, "eBRestarter");
        }

        [Fact]
        public void EnableAutoStart_ShouldCatchException_AndNotCrash()
        {
            // [E]RROR: Wraps registry access exceptions in InvalidOperationException with preserved inner cause
            // Arrange
            _mockProcessInfo.GetCurrentExecutablePath().Returns("dummy.exe");
            _mockRegistry
                .When(r => r.SetUserValue(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<object>()))
                .Do(_ => throw new UnauthorizedAccessException("Access Denied"));

            // Act
            Action act = () => _sut.EnableAutoStart();

            // Assert
            var exception = act.ShouldThrow<InvalidOperationException>();
            exception.InnerException.ShouldBeOfType<UnauthorizedAccessException>();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task IsAutoStartEnabledAsync_ShouldReturnCorrectStatus(bool isEnabledInRegistry)
        {
            // [R]IGHT / [B]OUNDARY: Determines whether application autostart entry exists among registry values
            // Arrange
            var fakeRegistryEntries = new Dictionary<string, object>();

            if (isEnabledInRegistry)
            {
                fakeRegistryEntries.Add("eBRestarter", @"C:\Path\eB.exe");
            }
            fakeRegistryEntries.Add("SomeOtherApp", @"C:\Other\app.exe");

            _mockRegistry.GetUserValues(RegistryPathRun).Returns(fakeRegistryEntries);

            // Act
            bool result = await _sut.IsAutoStartEnabledAsync();

            // Assert
            result.ShouldBe(isEnabledInRegistry);
        }

        [Theory]
        [InlineData(true, 1)]
        [InlineData(false, 0)]
        public void SetBrowserStartupBoost_ShouldWriteCorrectDWordValue(bool enable, int expectedDword)
        {
            // [R]IGHT: Writes integer DWORD value representing Edge startup boost state to HKLM policies
            // Act
            _sut.SetBrowserStartupBoost(enable);

            // Assert
            _mockRegistry.Received(1).SetSystemValue(
                RegistryPathEdgePolicies,
                "StartupBoostEnabled",
                expectedDword);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(0, false)]
        [InlineData(null, true)]
        [InlineData("InvalidString", true)]
        public void IsBrowserStartupBoostEnabled_ShouldReturnExpectedResult(object? registryValue, bool expectedResult)
        {
            // [R]IGHT / [B]OUNDARY: Reads Edge startup boost policy value and falls back to default true when absent or invalid
            // Arrange
            _mockRegistry.GetSystemValue(RegistryPathEdgePolicies, "StartupBoostEnabled")
                         .Returns(registryValue);

            // Act
            bool result = _sut.IsBrowserStartupBoostEnabled();

            // Assert
            result.ShouldBe(expectedResult);
        }

        [Theory]
        [InlineData(true, 0)]
        [InlineData(false, 2)]
        public void SetAutoLogon_ShouldSetInvertedPasswordLessValue(bool enableAutoLogon, int expectedDword)
        {
            // [R]IGHT / [I]NVERSE: Sets inverted PasswordLess build version DWORD value for autologon configuration
            // Act
            _sut.SetAutoLogon(enableAutoLogon);

            // Assert
            _mockRegistry.Received(1).SetSystemValue(
                RegistryPathPasswordLess,
                "DevicePasswordLessBuildVersion",
                expectedDword);
        }

        [Fact]
        public void SetAutoLogon_ShouldCatchUnauthorizedAccessException_Gracefully()
        {
            // [E]RROR: Throws UnauthorizedAccessException enriched with administrator privilege context
            // Arrange
            _mockRegistry
                .When(r => r.SetSystemValue(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<object>()))
                .Do(_ => throw new UnauthorizedAccessException("Need Admin Rights"));

            // Act
            Action act = () => _sut.SetAutoLogon(true);

            // Assert
            var exception = act.ShouldThrow<UnauthorizedAccessException>();
            exception.Message.ShouldContain("Administrator");
        }
    }
}
