using eBRestarter.Core.Application.Ports.Outbound.Network;
using eBRestarter.Core.Application.Ports.Outbound.Network;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Infrastructure.Adapters.Outbound.WindowsOS.Repositories;

namespace eBRestarter.Tests.Infrastructure.Services.WindowsOS
{
    /// <summary>
    /// Testet den WindowsStartupRepository.
    /// Dank der sauberen Architektur (ISettingsPort, IProcessInfoPort)
    /// k?nnen wir hier alle Registry-Zugriffe und Pfad-Ermittlungen vollst?ndig mocken,
    /// ohne das echte System des Entwicklers zu ver?ndern.
    /// </summary>
    public class WindowsStartupRepositoryTests
    {
        private readonly Mock<ILogger<AdapterWindowsStartupRepository>> _mockLogger;
        private readonly Mock<IOutboundPortSystemConfigurationRepository> _mockRegistry;
        private readonly Mock<IOutboundPortProcessInfoProvider> _mockProcessInfo;
        private readonly AdapterWindowsStartupRepository _sut;

        // Konstanten, die in der Originalklasse verwendet werden (f?r pr?zise Verification)
        private const string RegistryPathRun = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string RegistryPathEdgePolicies = @"SOFTWARE\Policies\Microsoft\Edge";
        private const string RegistryPathPasswordLess = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\PasswordLess\Device";

        public WindowsStartupRepositoryTests()
        {
            _mockLogger = new Mock<ILogger<AdapterWindowsStartupRepository>>();
            _mockRegistry = new Mock<IOutboundPortSystemConfigurationRepository>();
            _mockProcessInfo = new Mock<IOutboundPortProcessInfoProvider>();

            _sut = new AdapterWindowsStartupRepository(_mockLogger.Object, _mockRegistry.Object, _mockProcessInfo.Object);
        }
        // 1. AUTOSTART TESTS (Sync & Async)

        /// <summary>
        /// Stellt sicher, dass beim Aktivieren des Autostarts der exakte Pfad der
        /// laufenden .exe-Datei in den korrekten "Run"-Schl?ssel geschrieben wird.
        ///
        /// WAS WIRD GETESTET?
        /// Wir geben ?ber den ProcessInfo-Mock einen Fake-Pfad zur?ck und pr?fen,
        /// ob der Registry-Service exakt diesen Pfad speichert.
        /// </summary>
        [Fact]
        public void EnableAutoStart_ShouldSetRegistryKey_WithCurrentExePath()
        {
            // ARRANGE
            string fakeExePath = @"C:\TestApp\eBRestarter.exe";
            _mockProcessInfo.Setup(p => p.GetCurrentExecutablePath()).Returns(fakeExePath);

            // ACT
            _sut.EnableAutoStart();

            // ASSERT
            _mockRegistry.Verify(r => r.SetUserValue(
                RegistryPathRun,
                "eBRestarter",
                fakeExePath),
            Times.Once);
        }

        /// <summary>
        /// Stellt sicher, dass der Autostart-Eintrag korrekt aus der Registry gel?scht wird.
        /// </summary>
        [Fact]
        public void DisableAutoStart_ShouldDeleteRegistryKey()
        {
            // ACT
            _sut.DisableAutoStart();

            // ASSERT
            _mockRegistry.Verify(r => r.DeleteUserValue(RegistryPathRun, "eBRestarter"), Times.Once);
        }

        /// <summary>
        /// Wenn ein Fehler bei der Registry auftritt (z.B. fehlende Berechtigungen),
        /// darf die Anwendung nicht crashen. Der Fehler muss gefangen und geloggt werden.
        /// </summary>
        [Fact]
        public void EnableAutoStart_ShouldCatchException_AndNotCrash()
        {
            // ARRANGE
            _mockProcessInfo.Setup(p => p.GetCurrentExecutablePath()).Returns("dummy.exe");
            _mockRegistry.Setup(r => r.SetUserValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                         .Throws(new UnauthorizedAccessException("Access Denied"));

            // ACT & ASSERT
            // Darf keine Exception werfen
            Should.NotThrow(() => _sut.EnableAutoStart());
        }

        /// <summary>
        /// Pr?ft, ob IsAutoStartEnabledAsync korrekt auswertet, ob unser App-Schl?ssel
        /// in den geladenen Registry-Werten existiert.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task IsAutoStartEnabledAsync_ShouldReturnCorrectStatus(bool isEnabledInRegistry)
        {
            // ARRANGE
            var fakeRegistryEntries = new Dictionary<string, object>();

            if (isEnabledInRegistry)
            {
                fakeRegistryEntries.Add("eBRestarter", @"C:\Path\eB.exe");
            }
            // Wir f?gen Rauschen hinzu, um sicherzustellen, dass er nach dem richtigen Key sucht
            fakeRegistryEntries.Add("SomeOtherApp", @"C:\Other\app.exe");

            _mockRegistry.Setup(r => r.GetUserValues(RegistryPathRun)).Returns(fakeRegistryEntries);

            // ACT
            bool result = await _sut.IsAutoStartEnabledAsync();

            // ASSERT
            result.ShouldBe(isEnabledInRegistry);
        }
        // 2. EDGE STARTUP BOOST TESTS

        /// <summary>
        /// Stellt sicher, dass das Aktivieren/Deaktivieren des Edge-Boosts
        /// die korrekten Integer-Werte (1 = an, 0 = aus) als DWord in HKLM schreibt.
        /// </summary>
        [Theory]
        [InlineData(true, 1)]
        [InlineData(false, 0)]
        public void SetBrowserStartupBoost_ShouldWriteCorrectDWordValue(bool enable, int expectedDword)
        {
            // ACT
            _sut.SetBrowserStartupBoost(enable);

            // ASSERT
            _mockRegistry.Verify(r => r.SetSystemValue(
                RegistryPathEdgePolicies,
                "StartupBoostEnabled",
                expectedDword),
            Times.Once);
        }

        /// <summary>
        /// Pr?ft das Auslesen des Edge-Status. Standardverhalten laut deinem Code:
        /// 1 -> True, 0 -> False. Wenn nicht gesetzt (null) -> True.
        /// </summary>
        [Theory]
        [InlineData(1, true)]
        [InlineData(0, false)]
        [InlineData(null, true)] // Default Assumption in deinem Code
        [InlineData("InvalidString", true)] // Bei falschem Datentyp wird Fallback genutzt
        public void IsBrowserStartupBoostEnabled_ShouldReturnExpectedResult(object? registryValue, bool expectedResult)
        {
            // ARRANGE
            _mockRegistry.Setup(r => r.GetSystemValue(RegistryPathEdgePolicies, "StartupBoostEnabled"))
                         .Returns(registryValue);

            // ACT
            bool result = _sut.IsBrowserStartupBoostEnabled();

            // ASSERT
            result.ShouldBe(expectedResult);
        }
        // 3. AUTO LOGON TESTS (PasswordLess)

        /// <summary>
        /// AutoLogon und PasswordLess verhalten sich invers.
        /// Wenn AutoLogon aktiviert werden soll (true), muss PasswordLess deaktiviert werden (0).
        /// Wenn AutoLogon deaktiviert werden soll (false), muss PasswordLess aktiviert werden (2).
        /// </summary>
        [Theory]
        [InlineData(true, 0)]
        [InlineData(false, 2)]
        public void SetAutoLogon_ShouldSetInvertedPasswordLessValue(bool enableAutoLogon, int expectedDword)
        {
            // ACT
            _sut.SetAutoLogon(enableAutoLogon);

            // ASSERT
            _mockRegistry.Verify(r => r.SetSystemValue(
                RegistryPathPasswordLess,
                "DevicePasswordLessBuildVersion",
                expectedDword),
            Times.Once);
        }

        /// <summary>
        /// Wenn das Setzen des AutoLogon in HKLM ohne Adminrechte fehlschl?gt,
        /// darf der Prozess nicht abst?rzen, sondern muss das sauber loggen.
        /// </summary>
        [Fact]
        public void SetAutoLogon_ShouldCatchUnauthorizedAccessException_Gracefully()
        {
            // ARRANGE
            _mockRegistry.Setup(r => r.SetSystemValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                         .Throws(new UnauthorizedAccessException("Need Admin Rights"));

            // ACT & ASSERT
            // Die Methode f?ngt die Exception spezifisch ab, crasht also nicht.
            Should.NotThrow(() => _sut.SetAutoLogon(true));
        }
    }
}




