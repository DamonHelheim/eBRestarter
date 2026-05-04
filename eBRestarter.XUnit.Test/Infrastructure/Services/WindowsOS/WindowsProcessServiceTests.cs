using eBRestarter.Infrastructure.Services.WindowsOS;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS.Process;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace eBRestarter.Tests.Infrastructure.Services.WindowsOS
{
    /// <summary>
    /// Testet den WindowsProcessService.
    /// Dank des IProcessWrappers können wir alle Prozess-Starts, Kills und Checks simulieren,
    /// ohne das eigentliche Betriebssystem zu beeinflussen.
    /// </summary>
    public class WindowsProcessServiceTests
    {
        private readonly Mock<ILogger<WindowsProcessService>> _mockLogger;
        private readonly Mock<IProcessWrapper> _mockProcessWrapper;
        private readonly WindowsProcessService _sut;

        public WindowsProcessServiceTests()
        {
            _mockLogger = new Mock<ILogger<WindowsProcessService>>();
            _mockProcessWrapper = new Mock<IProcessWrapper>();

            _sut = new WindowsProcessService(_mockLogger.Object, _mockProcessWrapper.Object);
        }

        // =========================================================
        // 1. START EXECUTABLE TESTS
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Stellt sicher, dass das ProcessStartInfo-Objekt korrekt konfiguriert wird
        /// (korrekter Pfad und UseShellExecute = true).
        /// </summary>
        [Fact]
        public void StartExecutable_ShouldCallWrapper_WithCorrectStartInfo()
        {
            // ARRANGE
            string exePath = @"C:\TestApp\app.exe";

            // ACT
            _sut.StartExecutable(exePath);

            // ASSERT
            _mockProcessWrapper.Verify(w => w.Start(It.Is<ProcessStartInfo>(info =>
                info.FileName == exePath &&
                info.UseShellExecute == true
            )), Times.Once);
        }

        // =========================================================
        // 2. MSI INSTALLER TESTS
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Der MSI-Start ist komplex: Er muss den absoluten Pfad zur msiexec.exe nutzen (Security),
        /// Argumente setzen, Output umleiten und auf das Ende warten.
        ///
        /// WAS WIRD GETESTET?
        /// Wir simulieren einen startenden Prozess mit gefälschten Konsolen-Outputs (Streams)
        /// und prüfen, ob alle Eigenschaften korrekt gesetzt wurden und WaitForExit aufgerufen wird.
        /// </summary>
        [Fact]
        public void StartMsiFile_ShouldConfigureMsiExec_AndReadStreams()
        {
            // ARRANGE
            string msiPath = @"C:\Install\setup.msi";
            string expectedSystemFolder = Environment.GetFolderPath(Environment.SpecialFolder.System);
            string expectedMsiExecPath = Path.Combine(expectedSystemFolder, "msiexec.exe");

            // HIER NEU: Nutzt jetzt dein echtes IProcess Interface
            var mockProcess = new Mock<IProcess>();

            // Dummy-Streams für Output und Error
            var outStream = new MemoryStream(Encoding.UTF8.GetBytes("Installation OK"));
            var errStream = new MemoryStream(Encoding.UTF8.GetBytes(""));
            mockProcess.Setup(p => p.StandardOutput).Returns(new StreamReader(outStream));
            mockProcess.Setup(p => p.StandardError).Returns(new StreamReader(errStream));

            _mockProcessWrapper.Setup(w => w.Start(It.IsAny<ProcessStartInfo>())).Returns(mockProcess.Object);

            // ACT
            _sut.StartMsiFile(msiPath);

            // ASSERT
            // 1. Prüfen, ob der Wrapper mit den korrekten Parametern aufgerufen wurde
            _mockProcessWrapper.Verify(w => w.Start(It.Is<ProcessStartInfo>(info =>
                info.FileName == expectedMsiExecPath &&
                info.Arguments == $"/i \"{msiPath}\"" &&
                info.UseShellExecute == false &&
                info.RedirectStandardOutput == true &&
                info.CreateNoWindow == true
            )), Times.Once);

            // 2. Prüfen, ob auf das Beenden gewartet wurde
            mockProcess.Verify(p => p.WaitForExit(), Times.Once);
        }

        // =========================================================
        // 3. EXPLORER / BROWSER / SHUTDOWN TESTS
        // =========================================================

        [Fact]
        public void OpenUrlInBrowser_ShouldPassUrlAsArgument()
        {
            // ARRANGE
            string browserPath = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
            string url = "https://www.google.com";

            // ACT
            _sut.OpenUrlInBrowser(browserPath, url);

            // ASSERT
            _mockProcessWrapper.Verify(w => w.Start(It.Is<ProcessStartInfo>(info =>
                info.FileName == browserPath &&
                info.Arguments == url &&
                info.UseShellExecute == true
            )), Times.Once);
        }

        [Fact]
        public void ShutdownComputer_ShouldCallShutdownExe_WithForceParams()
        {
            // ARRANGE
            string expectedSystemFolder = Environment.GetFolderPath(Environment.SpecialFolder.System);
            string expectedShutdownPath = Path.Combine(expectedSystemFolder, "shutdown.exe");

            // ACT
            _sut.ShutdownComputer();

            // ASSERT
            _mockProcessWrapper.Verify(w => w.Start(It.Is<ProcessStartInfo>(info =>
                info.FileName == expectedShutdownPath &&
                info.Arguments == "/r /f /t 0"
            )), Times.Once);
        }

        [Fact]
        public void OpenExplorer_ShouldCallExplorerExe_WithQuotedPath()
        {
            // ARRANGE
            string targetFolder = @"C:\My Folder With Spaces";
            string expectedWindowsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            string expectedExplorerPath = Path.Combine(expectedWindowsFolder, "explorer.exe");

            // ACT
            _sut.OpenExplorer(targetFolder);

            // ASSERT
            _mockProcessWrapper.Verify(w => w.Start(It.Is<ProcessStartInfo>(info =>
                info.FileName == expectedExplorerPath &&
                info.Arguments == $"\"{targetFolder}\""
            )), Times.Once);
        }

        // =========================================================
        // 4. PROCESS MANAGEMENT TESTS (Kill, Check, Close)
        // =========================================================

        [Fact]
        public void CloseApplication_ShouldCallKill_WhenProcessIsRunning()
        {
            // ARRANGE
            string processName = "notepad";
            _mockProcessWrapper.Setup(w => w.IsProcessRunning(processName)).Returns(true);

            // ACT
            _sut.CloseApplication(processName);

            // ASSERT
            _mockProcessWrapper.Verify(w => w.KillProcess(processName), Times.Once);
        }

        [Fact]
        public void CloseApplication_ShouldDoNothing_WhenProcessIsNotRunning()
        {
            // ARRANGE
            string processName = "notepad";
            _mockProcessWrapper.Setup(w => w.IsProcessRunning(processName)).Returns(false);

            // ACT
            _sut.CloseApplication(processName);

            // ASSERT
            _mockProcessWrapper.Verify(w => w.KillProcess(It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Die "CloseAllOpenPrograms" Methode darf kritische Prozesse wie "System" oder "Idle"
        /// nicht anrühren und soll Prozesse ohne GUI (MainWindowHandle == 0) ignorieren.
        /// </summary>
        [Fact]
        public async Task CloseAllOpenProgramsAsync_ShouldIgnoreSystemAndHeadlessProcesses()
        {
            // ARRANGE
            // HIER NEU: IProcess anstelle von IWrappedProcess
            var systemMock = new Mock<IProcess>();
            systemMock.Setup(p => p.ProcessName).Returns("System");

            var headlessMock = new Mock<IProcess>();
            headlessMock.Setup(p => p.ProcessName).Returns("BackgroundService");
            headlessMock.Setup(p => p.MainWindowHandle).Returns(IntPtr.Zero); // Keine GUI

            var validAppMock = new Mock<IProcess>();
            validAppMock.Setup(p => p.ProcessName).Returns("Notepad");
            validAppMock.Setup(p => p.MainWindowHandle).Returns(new IntPtr(1234)); // Hat eine GUI
            validAppMock.Setup(p => p.WaitForExitAsync()).Returns(Task.CompletedTask);

            // HIER NEU: Gibt ein IProcess[] Array zurück
            _mockProcessWrapper.Setup(w => w.GetProcesses()).Returns(new IProcess[]
            {
                systemMock.Object,
                headlessMock.Object,
                validAppMock.Object
            });

            // ACT
            await _sut.CloseAllOpenProgramsAsync(5000);

            // ASSERT
            // Darf bei System nicht aufgerufen werden
            systemMock.Verify(p => p.WaitForExitAsync(), Times.Never);

            // Darf bei Headless nicht aufgerufen werden
            headlessMock.Verify(p => p.WaitForExitAsync(), Times.Never);

            // Bei der Valid App muss WaitForExitAsync getriggert worden sein
            validAppMock.Verify(p => p.WaitForExitAsync(), Times.Once);

            // Da wir 'using (process)' implementiert haben, muss Dispose aufgerufen worden sein
            validAppMock.Verify(p => p.Dispose(), Times.Once);
        }

        // =========================================================
        // 5. ASYNC PROCESS TESTS
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Prüft, ob der Task korrekt gewartet (await) wird.
        /// </summary>
        [Fact]
        public async Task StartExecutableAsync_ShouldAwaitProcessExit()
        {
            // ARRANGE
            // HIER NEU: IProcess
            var mockProcess = new Mock<IProcess>();
            mockProcess.Setup(p => p.WaitForExitAsync()).Returns(Task.CompletedTask);

            _mockProcessWrapper.Setup(w => w.Start(It.IsAny<ProcessStartInfo>())).Returns(mockProcess.Object);

            // ACT
            await _sut.StartExecutableAsync("test.exe");

            // ASSERT
            mockProcess.Verify(p => p.WaitForExitAsync(), Times.Once);
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Laut deinem Code wird die Exception in der Async-Methode geloggt UND per 'throw' weitergeworfen.
        /// Wir prüfen, ob diese Exception tatsächlich oben ankommt.
        /// </summary>
        [Fact]
        public async Task StartExecutableAsync_ShouldRethrowException()
        {
            // ARRANGE
            _mockProcessWrapper
                .Setup(w => w.Start(It.IsAny<ProcessStartInfo>()))
                .Throws(new InvalidOperationException("Access Denied"));

            // ACT
            Func<Task> act = async () => await _sut.StartExecutableAsync("test.exe");

            // ASSERT
            await act.ShouldThrowAsync<InvalidOperationException>();
        }
    }
}