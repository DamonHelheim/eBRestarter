using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Infrastructure.Services.WindowsOS;
using eBRestarter.Infrastructure.Wrapper.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace eBRestarter.Tests.Infrastructure.Services.WindowsOS
{
    /// <summary>
    /// Testet den WindowsStartupService.
    /// Dank der sauberen Architektur (IWindowsRegistryService, IProcessInfoService)
    /// können wir hier alle Registry-Zugriffe und Pfad-Ermittlungen vollständig mocken,
    /// ohne das echte System des Entwicklers zu verändern.
    /// </summary>
    public class WindowsStartupServiceTests
    {
        private readonly Mock<ILogger<WindowsStartupService>> _mockLogger;
        private readonly Mock<IWindowsRegistryService> _mockRegistry;
        private readonly Mock<IProcessInfoService> _mockProcessInfo;
        private readonly WindowsStartupService _sut;

        // Konstanten, die in der Originalklasse verwendet werden (für präzise Verification)
        private const string RegistryPathRun = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string RegistryPathEdgePolicies = @"SOFTWARE\Policies\Microsoft\Edge";
        private const string RegistryPathPasswordLess = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\PasswordLess\Device";

        public WindowsStartupServiceTests()
        {
            _mockLogger = new Mock<ILogger<WindowsStartupService>>();
            _mockRegistry = new Mock<IWindowsRegistryService>();
            _mockProcessInfo = new Mock<IProcessInfoService>();

            _sut = new WindowsStartupService(_mockLogger.Object, _mockRegistry.Object, _mockProcessInfo.Object);
        }

        // =========================================================
        // 1. AUTOSTART TESTS (Sync & Async)
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Stellt sicher, dass beim Aktivieren des Autostarts der exakte Pfad der
        /// laufenden .exe-Datei in den korrekten "Run"-Schlüssel geschrieben wird.
        ///
        /// WAS WIRD GETESTET?
        /// Wir geben über den ProcessInfo-Mock einen Fake-Pfad zurück und prüfen,
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
            _mockRegistry.Verify(r => r.SetCurrentUserValue(
                RegistryPathRun,
                "eBRestarter",
                fakeExePath),
            Times.Once);
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Stellt sicher, dass der Autostart-Eintrag korrekt aus der Registry gelöscht wird.
        /// </summary>
        [Fact]
        public void DisableAutoStart_ShouldDeleteRegistryKey()
        {
            // ACT
            _sut.DisableAutoStart();

            // ASSERT
            _mockRegistry.Verify(r => r.DeleteCurrentUserValue(RegistryPathRun, "eBRestarter"), Times.Once);
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Wenn ein Fehler bei der Registry auftritt (z.B. fehlende Berechtigungen),
        /// darf die Anwendung nicht crashen. Der Fehler muss gefangen und geloggt werden.
        /// </summary>
        [Fact]
        public void EnableAutoStart_ShouldCatchException_AndNotCrash()
        {
            // ARRANGE
            _mockProcessInfo.Setup(p => p.GetCurrentExecutablePath()).Returns("dummy.exe");
            _mockRegistry.Setup(r => r.SetCurrentUserValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
                         .Throws(new UnauthorizedAccessException("Access Denied"));

            // ACT & ASSERT
            // Darf keine Exception werfen
            Should.NotThrow(() => _sut.EnableAutoStart());
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Prüft, ob IsAutoStartEnabledAsync korrekt auswertet, ob unser App-Schlüssel
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
            // Wir fügen Rauschen hinzu, um sicherzustellen, dass er nach dem richtigen Key sucht
            fakeRegistryEntries.Add("SomeOtherApp", @"C:\Other\app.exe");

            _mockRegistry.Setup(r => r.GetCurrentUserValues(RegistryPathRun)).Returns(fakeRegistryEntries);

            // ACT
            bool result = await _sut.IsAutoStartEnabledAsync();

            // ASSERT
            result.ShouldBe(isEnabledInRegistry);
        }

        // =========================================================
        // 2. EDGE STARTUP BOOST TESTS
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Stellt sicher, dass das Aktivieren/Deaktivieren des Edge-Boosts
        /// die korrekten Integer-Werte (1 = an, 0 = aus) als DWord in HKLM schreibt.
        /// </summary>
        [Theory]
        [InlineData(true, 1)]
        [InlineData(false, 0)]
        public void SetEdgeStartupBoost_ShouldWriteCorrectDWordValue(bool enable, int expectedDword)
        {
            // ACT
            _sut.SetEdgeStartupBoost(enable);

            // ASSERT
            _mockRegistry.Verify(r => r.SetLocalMachineValue(
                RegistryPathEdgePolicies,
                "StartupBoostEnabled",
                expectedDword,
                RegistryValueKind.DWord),
            Times.Once);
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Prüft das Auslesen des Edge-Status. Standardverhalten laut deinem Code:
        /// 1 -> True, 0 -> False. Wenn nicht gesetzt (null) -> True.
        /// </summary>
        [Theory]
        [InlineData(1, true)]
        [InlineData(0, false)]
        [InlineData(null, true)] // Default Assumption in deinem Code
        [InlineData("InvalidString", true)] // Bei falschem Datentyp wird Fallback genutzt
        public void IsEdgeStartupBoostEnabled_ShouldReturnExpectedResult(object? registryValue, bool expectedResult)
        {
            // ARRANGE
            _mockRegistry.Setup(r => r.GetLocalMachineValue(RegistryPathEdgePolicies, "StartupBoostEnabled"))
                         .Returns(registryValue);

            // ACT
            bool result = _sut.IsEdgeStartupBoostEnabled();

            // ASSERT
            result.ShouldBe(expectedResult);
        }

        // =========================================================
        // 3. AUTO LOGON TESTS (PasswordLess)
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
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
            _mockRegistry.Verify(r => r.SetLocalMachineValue(
                RegistryPathPasswordLess,
                "DevicePasswordLessBuildVersion",
                expectedDword,
                RegistryValueKind.DWord),
            Times.Once);
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Wenn das Setzen des AutoLogon in HKLM ohne Adminrechte fehlschlägt,
        /// darf der Prozess nicht abstürzen, sondern muss das sauber loggen.
        /// </summary>
        [Fact]
        public void SetAutoLogon_ShouldCatchUnauthorizedAccessException_Gracefully()
        {
            // ARRANGE
            _mockRegistry.Setup(r => r.SetLocalMachineValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<RegistryValueKind>()))
                         .Throws(new UnauthorizedAccessException("Need Admin Rights"));

            // ACT & ASSERT
            // Die Methode fängt die Exception spezifisch ab, crasht also nicht.
            Should.NotThrow(() => _sut.SetAutoLogon(true));
        }
    }
}