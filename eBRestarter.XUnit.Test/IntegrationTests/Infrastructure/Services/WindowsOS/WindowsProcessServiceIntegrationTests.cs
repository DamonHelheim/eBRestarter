using eBRestarter.Infrastructure.Services.WindowsOS;
using eBRestarter.Infrastructure.Wrapper;
using Shouldly;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace eBRestarter.XUnit.Test.IntegrationTests.Infrastructure.Services.WindowsOS
{
    // IDisposable ermöglicht uns das Aufräumen (Teardown) nach dem Test
    public class WindowsProcessServiceIntegrationTests : IDisposable
    {
        private readonly WindowsProcessService _service;
        private const string TestAppName = "notepad";
        private const string TestAppPath = @"C:\Windows\System32\notepad.exe";

        public WindowsProcessServiceIntegrationTests()
        {
            // 1. Vorbereitung: Wir stellen sicher, dass Notepad NICHT läuft, 
            // damit wir einen sauberen Startzustand haben.
            KillExistingNotepads();

            // 2. Setup: Wir nutzen die ECHTEN Klassen!
            // NullLogger verwirft die Logs (wir wollen sie im Test nicht sehen)
            var logger = NullLogger<WindowsProcessService>.Instance;
            var realWrapper = new RealProcessWrapper();

            _service = new WindowsProcessService(logger, realWrapper);
        }

        [Fact]
        [Trait("Category", "Integration")] // Hilft, diese Tests im Test-Explorer zu filtern
        public void StartExecutable_And_IsProcessAlive_Should_Work_With_Real_Windows()
        {
            // Act
            _service.StartExecutable(TestAppPath);

            // WICHTIG: Windows braucht Zeit! 
            // Der Prozess ist nicht in 0 Millisekunden da. Wir warten kurz.
            Thread.Sleep(1000);

            // Assert
            bool isRunning = _service.IsProcessAlive(TestAppName);
            isRunning.ShouldBeTrue("weil Notepad gerade gestartet wurde");
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CloseApplication_Should_Actually_Kill_Notepad()
        {
            // Arrange: Wir starten Notepad manuell (oder über den Service)
            Process.Start(TestAppPath);
            Thread.Sleep(1000); // Warten bis es da ist

            // Sicherstellen, dass es wirklich läuft
            _service.IsProcessAlive(TestAppName).ShouldBeTrue();

            // Act
            _service.CloseApplication(TestAppName);
            Thread.Sleep(500); // Warten bis Windows aufgeräumt hat

            // Assert
            bool isRunning = _service.IsProcessAlive(TestAppName);
            isRunning.ShouldBeFalse("weil CloseApplication den Prozess beenden sollte");
        }

        // Cleanup: Wird nach JEDEM Test ausgeführt
        public void Dispose()
        {
            // Wir räumen auf, falls ein Test fehlschlägt und Notepad offen lässt.
            KillExistingNotepads();
        }

        private void KillExistingNotepads()
        {
            foreach (var p in Process.GetProcessesByName(TestAppName))
            {
                try { p.Kill(); } catch { }
            }
        }
    }
}
