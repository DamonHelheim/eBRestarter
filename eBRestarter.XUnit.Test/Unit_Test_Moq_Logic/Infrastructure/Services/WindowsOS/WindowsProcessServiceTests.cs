using eBRestarter.Infrastructure.Services.WindowsOS;
using eBRestarter.Infrastructure.Wrapper.Interface;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using System.Diagnostics;

namespace eBRestarter.XUnit.Test.Unit_Test_Moq_Logic.Infrastructure.Services.Windows
{
    public class WindowsProcessServiceTests
    {
        // Mocks (Attrappen)
        private readonly Mock<ILogger<WindowsProcessService>> _loggerMock;
        private readonly Mock<IProcessWrapper> _wrapperMock;

        // System Under Test (Die Klasse, die wir testen)
        private readonly WindowsProcessService _sut;

        public WindowsProcessServiceTests()
        {
            _loggerMock = new Mock<ILogger<WindowsProcessService>>();
            _wrapperMock = new Mock<IProcessWrapper>();

            // Wir injizieren die Mocks in den Service
            _sut = new WindowsProcessService(_loggerMock.Object, _wrapperMock.Object);
        }

        #region StartExecutable Tests

        [Fact]
        public void StartExecutable_Should_CallWrapper_With_Correct_Arguments()
        {
            // Arrange
            string testPath = @"C:\Apps\test.exe";

            // Act
            _sut.StartExecutable(testPath);

            // Assert
            // Wir prüfen, ob Start() mit exakt diesen Parametern aufgerufen wurde
            _wrapperMock.Verify(x => x.Start(It.Is<ProcessStartInfo>(p =>
                p.FileName == testPath &&
                p.UseShellExecute == true
            )), Times.Once);

            // Prüfen, ob eine Info geloggt wurde
            VerifyLog(LogLevel.Information, "Executable gestartet");
        }

        [Fact]
        public void StartExecutable_Should_Catch_Exception_And_Log_Error()
        {
            // Arrange
            // Wir simulieren einen Fehler im Wrapper (z. B. Datei nicht gefunden)
            _wrapperMock.Setup(x => x.Start(It.IsAny<ProcessStartInfo>()))
                        .Throws(new System.ComponentModel.Win32Exception("Access denied"));

            // Act
            // Der Service darf nicht abstürzen, er muss den Fehler fangen
            var action = () => _sut.StartExecutable("test.exe");

            // Assert
            action.ShouldNotThrow(); // FluentAssertions
            VerifyLog(LogLevel.Error, "Fehler beim Starten");
        }

        #endregion

        #region ShutdownComputer Tests

        [Fact]
        public void ShutdownComputer_Should_Call_Shutdown_With_Reboot_Arguments()
        {
            // Act
            _sut.ShutdownComputer();

            // Assert
            // Wir prüfen, ob shutdown.exe mit /r /f /t 0 aufgerufen wird
            _wrapperMock.Verify(x => x.Start(It.Is<ProcessStartInfo>(p =>
                p.FileName == "shutdown" &&
                p.Arguments.Contains("/r") &&
                p.Arguments.Contains("/f") &&
                p.Arguments.Contains("/t 0")
            )), Times.Once);
        }

        #endregion

        #region CloseApplication Tests (Logik-Test!)

        [Fact]
        public void CloseApplication_Should_Kill_Process_If_It_Is_Running()
        {
            // Arrange
            string appName = "Notepad";
            // Wir sagen dem Mock: "Ja, der Prozess läuft"
            _wrapperMock.Setup(x => x.IsProcessRunning(appName)).Returns(true);

            // Act
            _sut.CloseApplication(appName);

            // Assert
            // KillProcess MUSS aufgerufen werden
            _wrapperMock.Verify(x => x.KillProcess(appName), Times.Once);
        }

        [Fact]
        public void CloseApplication_Should_Do_Nothing_If_Process_Is_Not_Running()
        {
            // Arrange
            string appName = "GhostApp";
            // Wir sagen dem Mock: "Nein, läuft nicht"
            _wrapperMock.Setup(x => x.IsProcessRunning(appName)).Returns(false);

            // Act
            _sut.CloseApplication(appName);

            // Assert
            // KillProcess darf NICHT aufgerufen werden
            _wrapperMock.Verify(x => x.KillProcess(It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region StartMsiFile Tests

        [Fact]
        public void StartMsiFile_Should_Log_Warning_If_Process_Is_Null()
        {
            // Arrange
            // Wir simulieren, dass Process.Start null zurückgibt (z.B. Fehler beim Start)
            _wrapperMock.Setup(x => x.Start(It.IsAny<ProcessStartInfo>())).Returns((Process?)null);

            // Act
            _sut.StartMsiFile("installer.msi");

            // Assert
            VerifyLog(LogLevel.Warning, "konnte nicht gestartet werden");
        }

        // Hinweis: Der "Happy Path" von StartMsiFile ist schwer als Unit Test zu testen,
        // da man die Streams (StandardOutput) eines echten Process-Objekts schwer mocken kann.
        // Hier wäre ein Integrationstest besser geeignet.

        #endregion

        #region IsProcessAlive Tests

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsProcessAlive_Should_Return_Result_From_Wrapper(bool isRunning)
        {
            // Arrange
            _wrapperMock.Setup(x => x.IsProcessRunning("test")).Returns(isRunning);

            // Act
            bool result = _sut.IsProcessAlive("test");

            // Assert
            result.ShouldBe(isRunning);
        }

        #endregion

        // --- Hilfsmethode für Logger-Verifikation ---
        // Da LogInformation/LogError Extension Methods sind, muss man Log(...) direkt prüfen.
        private void VerifyLog(LogLevel level, string messageFragment)
        {
            _loggerMock.Verify(
                x => x.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(messageFragment)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
