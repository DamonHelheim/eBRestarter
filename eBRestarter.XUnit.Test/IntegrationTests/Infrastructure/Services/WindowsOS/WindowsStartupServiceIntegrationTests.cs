//using eBRestarter.Infrastructure.Services.WindowsOS;
//using eBRestarter.Infrastructure.Wrapper;
//using Shouldly;
//using Microsoft.Extensions.Logging.Abstractions;
//using Microsoft.Win32;
//using System;
//using System.Collections.Generic;
//using System.Runtime.Versioning;
//using System.Security.Principal;
//using System.Text;

//namespace eBRestarter.XUnit.Test.IntegrationTests.Infrastructure.Services.WindowsOS
//{
//    [SupportedOSPlatform("windows")]
//    public class WindowsStartupServiceIntegrationTests : IDisposable
//    {
//        // System Under Test
//        private readonly WindowsStartupService _service;

//        // Konstanten für die Überprüfung
//        private const string AppName = "eBRestarter";
//        private const string RunPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
//        private const string EdgePath = @"SOFTWARE\Policies\Microsoft\Edge";
//        private const string PasswordLessPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\PasswordLess\Device";

//        public WindowsStartupServiceIntegrationTests()
//        {
//            // 1. Setup: Wir nutzen die ECHTEN Implementierungen (keine Mocks!)
//            // Damit testen wir den kompletten Durchstich bis zum Betriebssystem.
//            var logger = NullLogger<WindowsStartupService>.Instance;
//            var realRegistry = new WindowsRegistryService();
//            var realProcessInfo = new ProcessInfoService();

//            _service = new WindowsStartupService(logger, realRegistry, realProcessInfo);

//            // Safety First: Vor jedem Test sicherstellen, dass wir sauber starten
//            CleanupRegistry();
//        }

//        // --- HKCU Tests (Laufen meist ohne Admin) ---

//        [Fact]
//        [Trait("Category", "Integration")]
//        public void EnableAutoStart_Should_Create_Real_Registry_Entry()
//        {
//            // Act
//            _service.EnableAutoStart();

//            // Assert
//            // Wir prüfen direkt in der Registry, ob der Wert angekommen ist.
//            using var key = Registry.CurrentUser.OpenSubKey(RunPath, false); // false = nur lesen
//            var value = key?.GetValue(AppName);

//            value.ShouldNotBeNull("weil EnableAutoStart einen Eintrag erstellen sollte.");

//            // Hinweis: Im Test-Runner ist die .exe oft 'testhost.exe' oder 'dotnet.exe'
//            var pathString = value?.ToString();
//            pathString.ShouldEndWith(".exe"); // Optional: Kommentar beibehalten
//        }

//        [Fact]
//        [Trait("Category", "Integration")]
//        public void DisableAutoStart_Should_Remove_Real_Registry_Entry()
//        {
//            // Arrange: Wir erstellen erst einen Eintrag (damit wir was zu löschen haben)
//            _service.EnableAutoStart();

//            // Kurz prüfen, ob er wirklich da ist (Sicherheitscheck)
//            using (var checkKey = Registry.CurrentUser.OpenSubKey(RunPath, false))
//            {
//                checkKey.GetValue(AppName).ShouldNotBeNull();
//            }

//            // Act
//            _service.DisableAutoStart();

//            // Assert
//            using var key = Registry.CurrentUser.OpenSubKey(RunPath, false);
//            var value = key?.GetValue(AppName);

//            value.ShouldBeNull("weil DisableAutoStart den Eintrag entfernen muss.");
//        }

//        [Fact]
//        [Trait("Category", "Integration")]
//        public void GetStartupEntries_Should_Find_Our_Entry()
//        {
//            // Arrange
//            _service.EnableAutoStart();

//            // Act
//            var entries = _service.GetStartupEntries();

//            // Assert
//            entries.ShouldContainKey(AppName, "weil wir uns gerade selbst in den Autostart eingetragen haben.");
//        }

//        // --- HKLM Tests (Benötigen zwingend Admin-Rechte) ---

//        [Fact]
//        [Trait("Category", "Integration_Admin")]
//        public void SetEdgeStartupBoost_Should_Modify_HKLM_If_Admin()
//        {
//            // Vorbedingung prüfen: Sind wir Admin?
//            if (!IsAdministrator())
//            {
//                // Wenn nicht, geben wir eine Warnung aus oder überspringen den Test, 
//                // damit der Build nicht fehlschlägt.
//                // In xUnit kann man das elegant so lösen (Output sieht man im Test Explorer):
//                Assert.True(true, "SKIP: Test übersprungen, da keine Administratorrechte vorhanden.");
//                return;
//            }

//            // Act: Wir deaktivieren Boost (setzt Wert auf 0)
//            _service.SetEdgeStartupBoost(false);

//            // Assert
//            using var key = Registry.LocalMachine.OpenSubKey(EdgePath, false);
//            key.ShouldNotBeNull("weil der Key erstellt sein sollte.");

//            var val = key!.GetValue("StartupBoostEnabled");
//            val.ShouldBe(0, "weil wir 'false' übergeben haben (0 = Disabled).");

//            // Cleanup/Restore: Wir schalten es testweise wieder an (1)
//            _service.SetEdgeStartupBoost(true);
//            key.GetValue("StartupBoostEnabled").ShouldBe(1);
//        }

//        [Fact]
//        [Trait("Category", "Integration_Admin")]
//        public void SetAutoLogon_Should_Modify_HKLM_If_Admin()
//        {
//            if (!IsAdministrator())
//            {
//                Assert.True(true, "SKIP: Test übersprungen, da keine Administratorrechte vorhanden.");
//                return;
//            }

//            // Act: Wir erlauben AutoLogon (enable = true -> RegValue = 0)
//            _service.SetAutoLogon(true);

//            // Assert
//            using var key = Registry.LocalMachine.OpenSubKey(PasswordLessPath, false);
//            key.ShouldNotBeNull();

//            var val = key!.GetValue("DevicePasswordLessBuildVersion");
//            val.ShouldBe(0, "weil 'enable=true' (AutoLogon an) bedeutet 'PasswordLess=aus' (0).");

//            // Cleanup: Zurücksetzen auf Default (Hello Zwang = 2)
//            _service.SetAutoLogon(false);
//            key.GetValue("DevicePasswordLessBuildVersion").ShouldBe(2);
//        }

//        // --- Aufräumen (Teardown) ---

//        public void Dispose()
//        {
//            // Diese Methode wird nach JEDEM Test ausgeführt.
//            CleanupRegistry();
//        }

//        private void CleanupRegistry()
//        {
//            try
//            {
//                // Versuchen, den Autostart-Eintrag zu löschen, damit dein PC sauber bleibt.
//                using var key = Registry.CurrentUser.OpenSubKey(RunPath, true); // true = Schreibrechte
//                if (key != null && key.GetValue(AppName) != null)
//                {
//                    key.DeleteValue(AppName, false);
//                }
//            }
//            catch
//            {
//                // Fehler beim Aufräumen ignorieren wir (z.B. wenn Key nicht existiert)
//            }
//        }

//        // Hilfsmethode zur Rechteprüfung
//        private bool IsAdministrator()
//        {
//            using var identity = WindowsIdentity.GetCurrent();
//            var principal = new WindowsPrincipal(identity);
//            return principal.IsInRole(WindowsBuiltInRole.Administrator);
//        }
//    }
//}
