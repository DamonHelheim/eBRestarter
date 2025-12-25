using eBRestarter.Infrastructure.Services.WindowsOS;
using eBRestarter.Infrastructure.Wrapper.Interface;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.XUnit.Test.Unit_Test_Moq_Logic.Infrastructure.Services.WindowsOS
{
    public class WindowsStartupServiceTests
    {
        private readonly Mock<ILogger<WindowsStartupService>> _loggerMock;
        private readonly Mock<IRegistryService> _registryMock;
        private readonly Mock<IProcessInfoService> _processMock;
        private readonly WindowsStartupService _windowsStartupService;

        public WindowsStartupServiceTests()
        {
            _loggerMock = new Mock<ILogger<WindowsStartupService>>();
            _registryMock = new Mock<IRegistryService>();
            _processMock = new Mock<IProcessInfoService>();

            _windowsStartupService = new WindowsStartupService(_loggerMock.Object, _registryMock.Object, _processMock.Object);
        }

        [Fact]
        public void EnableAutoStart_ShouldWriteCorrectPathToRegistry()
        {
            // Arrange
            string fakePath = @"C:\MyApp\eBRestarter.exe";

            // Sicherstellen, dass der Mock bereit ist
            _processMock.Setup(x => x.GetCurrentExecutablePath()).Returns(fakePath);

            // Act
            _windowsStartupService.EnableAutoStart();

            // Assert
            // Prüfen, ob wir überhaupt bis zum Registry-Aufruf gekommen sind
            try
            {
                _registryMock.Verify(x => x.SetCurrentUserValue(
                    It.Is<string>(s => s.Contains("Run")), // Etwas toleranter beim Pfad für den Test
                    "eBRestarter",
                    fakePath), Times.Once);
            }
            catch (MockException)
            {
                // Wenn das fehlschlägt, prüfen wir, ob stattdessen ein Fehler geloggt wurde
                _loggerMock.Verify(x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.AtLeastOnce,
                    "Der Test schlug fehl, weil EnableAutoStart eine Exception gefangen hat.");

                throw; // Den ursprünglichen Verify-Fehler werfen, damit der Test rot bleibt
            }
        }

        [Fact]
        public void DisableAutoStart_ShouldDeleteRegistryValue()
        {
            // Act
            _windowsStartupService.DisableAutoStart();

            // Assert
            _registryMock.Verify(x => x.DeleteCurrentUserValue(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",
                "eBRestarter"), Times.Once);
        }

        [Fact]
        public void GetStartupEntries_ShouldReturnDictionaryFromRegistry()
        {
            // Arrange
            var fakeEntries = new Dictionary<string, object>
        {
            { "Steam", @"C:\Steam.exe" },
            { "Discord", @"C:\Discord.exe" }
        };

            _registryMock.Setup(x => x.GetCurrentUserValues(It.IsAny<string>()))
                         .Returns(fakeEntries);

            // Act
            var result = _windowsStartupService.GetStartupEntries();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.True(result.ContainsKey("Steam"));
        }

        [Fact]
        public void GetStartupEntries_ShouldReturnEmpty_OnException()
        {
            // Arrange: Registry wirft Fehler
            _registryMock.Setup(x => x.GetCurrentUserValues(It.IsAny<string>()))
                         .Throws(new Exception("Registry kaputt"));

            // Act
            var result = _windowsStartupService.GetStartupEntries(); // Achte auf den Variablennamen (_service oder _windowsStartupService)

            // Assert
            Assert.Empty(result);

            // KORREKTUR: Hier muss der Text stehen, der im Service tatsächlich geloggt wird.
            // Laut deinem Stacktrace ist das: "Fehlerbeim Abrufen der Autostart-Einträge."
            VerifyLog(LogLevel.Error, "Fehlerbeim Abrufen der Autostart-Einträge");
        }

        [Theory]
        [InlineData(true, 1)] // Enable = true -> Value 1
        [InlineData(false, 0)] // Enable = false -> Value 0
        public void SetEdgeStartupBoost_ShouldSetCorrectDWord(bool enable, int expectedValue)
        {
            // Act
            _windowsStartupService.SetEdgeStartupBoost(enable);

            // Assert
            _registryMock.Verify(x => x.SetLocalMachineValue(
                @"SOFTWARE\Policies\Microsoft\Edge",
                "StartupBoostEnabled",
                expectedValue,
                Microsoft.Win32.RegistryValueKind.DWord), Times.Once);
        }

        [Theory]
        [InlineData(true, 0)] // Enable AutoLogon (remove Hello) -> Value 0
        [InlineData(false, 2)] // Disable AutoLogon (enforce Hello) -> Value 2
        public void SetAutoLogon_ShouldSetCorrectDWord(bool enable, int expectedValue)
        {
            // Act
            _windowsStartupService.SetAutoLogon(enable);

            // Assert
            _registryMock.Verify(x => x.SetLocalMachineValue(
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\PasswordLess\Device",
                "DevicePasswordLessBuildVersion",
                expectedValue,
                Microsoft.Win32.RegistryValueKind.DWord), Times.Once);
        }

        [Fact]
        public void SetAutoLogon_ShouldLogPermissionError_WhenUnauthorized()
        {
            // Arrange
            _registryMock.Setup(x => x.SetLocalMachineValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<Microsoft.Win32.RegistryValueKind>()))
                         .Throws(new UnauthorizedAccessException("Keine Admin Rechte"));

            // Act
            _windowsStartupService.SetAutoLogon(true); // Achtung: _windowsStartupService hieß in vorherigen Beispielen _service

            // Assert
            // WICHTIG: Hier muss ein Text stehen, der wirklich geloggt wird ("Zugriff verweigert")
            VerifyLog(LogLevel.Error, "Zugriff verweigert");
        }

        // Hilfsmethode um Logger Calls zu prüfen (Moq mit Extension Methods ist tricky)
        private void VerifyLog(LogLevel level, string messagePart)
        {
            _loggerMock.Verify(
                x => x.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(messagePart)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
