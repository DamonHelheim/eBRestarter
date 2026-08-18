using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using eBRestarter.Core.Application.Common.Results;
using eBRestarter.Core.Application.ObjectArchetypes.Constants;
using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Wrapper.WindowsOS;
using eBRestarter.Infrastructure.BehavioralComponents.Wrappers;
using NSubstitute.ExceptionExtensions;
using Microsoft.Extensions.Logging.Testing;

namespace eBRestarter.Tests.Infrastructure.Services.WindowsOS
{
    /// <summary>
    /// Unit tests for <see cref="AdapterWindowsProcessControlWrapper"/> verifying process execution, installer execution, browser/directory opening, process termination, and asynchronous lifecycle management.
    /// </summary>
    public class WindowsProcessServiceTests
    {
        private readonly FakeLogger<AdapterWindowsProcessControlWrapper> _mockLogger;
        private readonly IProcessWrapper _mockProcessWrapper;
        private readonly AdapterWindowsProcessControlWrapper _sut;

        public WindowsProcessServiceTests()
        {
            _mockLogger = new FakeLogger<AdapterWindowsProcessControlWrapper>();
            _mockProcessWrapper = Substitute.For<IProcessWrapper>();

            _sut = new AdapterWindowsProcessControlWrapper(_mockLogger, _mockProcessWrapper);
        }

        [Fact]
        public void StartExecutable_ShouldCallWrapper_WithCorrectStartInfo()
        {
            // [R]IGHT: Configures ProcessStartInfo with specified file path and shell execution enabled
            // Arrange
            string exePath = @"C:\TestApp\app.exe";

            // Act
            _sut.StartExecutable(exePath);

            // Assert
            _mockProcessWrapper.Received(1).Start(Arg.Is<ProcessStartInfo>(info =>
                info.FileName == exePath &&
                info.UseShellExecute == true
            ));
        }

        [Fact]
        public void RunInstaller_ShouldConfigureMsiExec_AndReadStreams()
        {
            // [R]IGHT / [B]OUNDARY: Configures msiexec with absolute system path, redirects output streams, and awaits exit
            // Arrange
            string msiPath = @"C:\Install\setup.msi";
            string expectedSystemFolder = Environment.GetFolderPath(Environment.SpecialFolder.System);
            string expectedMsiExecPath = Path.Combine(expectedSystemFolder, "msiexec.exe");
            var mockProcess = Substitute.For<IProcess>();

            var outStream = new MemoryStream(Encoding.UTF8.GetBytes("Installation OK"));
            var errStream = new MemoryStream(Encoding.UTF8.GetBytes(""));
            mockProcess.StandardOutput.Returns(new StreamReader(outStream));
            mockProcess.StandardError.Returns(new StreamReader(errStream));

            _mockProcessWrapper.Start(Arg.Any<ProcessStartInfo>()).Returns(mockProcess);

            // Act
            _sut.RunInstaller(msiPath);

            // Assert
            _mockProcessWrapper.Received(1).Start(Arg.Is<ProcessStartInfo>(info =>
                info.FileName == expectedMsiExecPath &&
                info.Arguments == $"/i \"{msiPath}\"" &&
                info.UseShellExecute == false &&
                info.RedirectStandardOutput == true &&
                info.CreateNoWindow == true
            ));

            mockProcess.Received(1).WaitForExit();
        }

        [Fact]
        public void OpenUrlInBrowser_ShouldPassUrlAsArgument()
        {
            // [R]IGHT: Launches browser executable with target URL as command-line argument
            // Arrange
            string browserPath = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
            string url = "https://www.google.com";

            // Act
            _sut.OpenUrlInBrowser(browserPath, url);

            // Assert
            _mockProcessWrapper.Received(1).Start(Arg.Is<ProcessStartInfo>(info =>
                info.FileName == browserPath &&
                info.Arguments == url &&
                info.UseShellExecute == true
            ));
        }

        [Fact]
        public void ShutdownComputer_ShouldCallShutdownExe_WithForceParams()
        {
            // [R]IGHT: Executes system shutdown with reboot and force parameters
            // Arrange
            string expectedSystemFolder = Environment.GetFolderPath(Environment.SpecialFolder.System);
            string expectedShutdownPath = Path.Combine(expectedSystemFolder, "shutdown.exe");

            // Act
            _sut.ShutdownComputer();

            // Assert
            _mockProcessWrapper.Received(1).Start(Arg.Is<ProcessStartInfo>(info =>
                info.FileName == expectedShutdownPath &&
                info.Arguments == "/r /f /t 0"
            ));
        }

        [Fact]
        public void OpenDirectoryInFileBrowser_ShouldCallExplorerExe_WithQuotedPath()
        {
            // [R]IGHT: Launches Windows Explorer with quoted folder directory argument
            // Arrange
            string targetFolder = @"C:\My Folder With Spaces";
            string expectedWindowsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            string expectedExplorerPath = Path.Combine(expectedWindowsFolder, "explorer.exe");

            // Act
            _sut.OpenDirectoryInFileBrowser(targetFolder);

            // Assert
            _mockProcessWrapper.Received(1).Start(Arg.Is<ProcessStartInfo>(info =>
                info.FileName == expectedExplorerPath &&
                info.Arguments == $"\"{targetFolder}\""
            ));
        }

        [Fact]
        public void CloseApplication_ShouldCallKill_WhenProcessIsRunning()
        {
            // [R]IGHT: Terminates process when matching running process is found
            // Arrange
            string processName = "notepad";
            _mockProcessWrapper.IsProcessRunning(processName).Returns(true);

            // Act
            _sut.CloseApplication(processName);

            // Assert
            _mockProcessWrapper.Received(1).KillProcess(processName);
        }

        [Fact]
        public void CloseApplication_ShouldDoNothing_WhenProcessIsNotRunning()
        {
            // [B]OUNDARY: Takes no action when target process is not currently running
            // Arrange
            string processName = "notepad";
            _mockProcessWrapper.IsProcessRunning(processName).Returns(false);

            // Act
            _sut.CloseApplication(processName);

            // Assert
            _mockProcessWrapper.DidNotReceive().KillProcess(Arg.Any<string>());
        }

        [Fact]
        public async Task CloseAllOpenProgramsAsync_ShouldIgnoreSystemAndHeadlessProcesses()
        {
            // [R]IGHT / [B]OUNDARY: Ignores protected system processes and headless services while closing windowed applications
            // Arrange
            var systemMock = Substitute.For<IProcess>();
            systemMock.ProcessName.Returns("System");

            var headlessMock = Substitute.For<IProcess>();
            headlessMock.ProcessName.Returns("BackgroundService");
            headlessMock.MainWindowHandle.Returns(IntPtr.Zero);

            var validAppMock = Substitute.For<IProcess>();
            validAppMock.ProcessName.Returns("Notepad");
            validAppMock.MainWindowHandle.Returns(new IntPtr(1234));
            validAppMock.WaitForExitAsync().Returns(Task.CompletedTask);
            _mockProcessWrapper.GetProcesses().Returns(new IProcess[]
            {
                systemMock,
                headlessMock,
                validAppMock
            });

            // Act
            await _sut.CloseAllOpenProgramsAsync(5000);

            // Assert
            await systemMock.DidNotReceive().WaitForExitAsync();
            await headlessMock.DidNotReceive().WaitForExitAsync();
            await validAppMock.Received(1).WaitForExitAsync();
            validAppMock.Received(1).Dispose();
        }

        [Fact]
        public async Task StartExecutableAsync_ShouldAwaitProcessExit()
        {
            // [R]IGHT: Starts asynchronous process and awaits completion
            // Arrange
            var mockProcess = Substitute.For<IProcess>();
            mockProcess.WaitForExitAsync().Returns(Task.CompletedTask);

            _mockProcessWrapper.Start(Arg.Any<ProcessStartInfo>()).Returns(mockProcess);

            // Act
            await _sut.StartExecutableAsync("test.exe");

            // Assert
            await mockProcess.Received(1).WaitForExitAsync();
        }

        [Fact]
        public async Task StartExecutableAsync_ShouldRethrowException()
        {
            // [E]RROR: Rethrows exceptions encountered during asynchronous process startup
            // Arrange
            _mockProcessWrapper
                .Start(Arg.Any<ProcessStartInfo>())
                .Throws(new InvalidOperationException("Access Denied"));

            // Act
            Func<Task> act = async () => await _sut.StartExecutableAsync("test.exe");

            // Assert
            await act.ShouldThrowAsync<InvalidOperationException>();
        }
    }
}
