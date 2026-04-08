using eBRestarter.Infrastructure.Services.WindowsOS;
using Microsoft.Win32;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Security;
using Xunit;

namespace eBRestarter.Tests.Infrastructure.Services.WindowsOS
{
    /// <summary>
    /// Testet den WindowsRegistryService.
    /// Da diese Klasse tief in das System eingreift, arbeiten wir für Schreibtests
    /// in einem sicheren, temporären Sandbox-Schlüssel unter HKEY_CURRENT_USER.
    /// </summary>
    public class WindowsRegistryServiceTests : IDisposable
    {
        private readonly WindowsRegistryService _sut;
        private readonly string _tempTestKey;

        public WindowsRegistryServiceTests()
        {
            _sut = new WindowsRegistryService();

            // Wir generieren für jeden Testdurchlauf einen einzigartigen, temporären Registry-Key.
            // Das verhindert, dass sich parallele Tests stören oder das Entwickler-System zugemüllt wird.
            _tempTestKey = $@"Software\eBRestarter_TestSandbox_{Guid.NewGuid()}";
        }

        // =========================================================
        // 1. CURRENT USER TESTS (HKCU) - Schreiben, Lesen, Löschen
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Stellt sicher, dass Werte korrekt in die CurrentUser-Registry geschrieben
        /// und exakt so wieder ausgelesen werden können.
        ///
        /// WAS WIRD GETESTET?
        /// Wir schreiben einen String in unsere Sandbox und lesen ihn über die Get-Methode wieder aus.
        /// </summary>
        [Fact]
        public void SetAndGetCurrentUserValue_ShouldWriteAndReadCorrectly()
        {
            // ARRANGE
            string valueName = "TestString";
            string expectedValue = "Hallo Registry!";

            // ACT
            _sut.SetCurrentUserValue(_tempTestKey, valueName, expectedValue);
            var actualValue = _sut.GetCurrentUserValue(_tempTestKey, valueName);

            // ASSERT
            actualValue.ShouldNotBeNull();
            actualValue.ToString().ShouldBe(expectedValue);
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Wenn ein Wert gelöscht wird, darf er danach nicht mehr existieren. Die Methode
        /// darf außerdem nicht abstürzen, wenn der Wert bereits fehlt (throwOnMissingValue: false).
        /// </summary>
        [Fact]
        public void DeleteCurrentUserValue_ShouldRemoveValue_WithoutCrashing()
        {
            // ARRANGE
            string valueName = "DeleteMe";
            _sut.SetCurrentUserValue(_tempTestKey, valueName, "Trash"); // Vorher anlegen

            // ACT
            _sut.DeleteCurrentUserValue(_tempTestKey, valueName);
            var resultAfterDelete = _sut.GetCurrentUserValue(_tempTestKey, valueName);

            // ASSERT
            resultAfterDelete.ShouldBeNull();

            // Zweiter Aufruf darf KEINEN Fehler werfen (Test der Robustheit)
            Should.NotThrow(() => _sut.DeleteCurrentUserValue(_tempTestKey, valueName));
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Stellt sicher, dass das Auslesen eines gesamten Registry-Schlüssels
        /// alle darin enthaltenen Werte als Dictionary zurückgibt.
        /// </summary>
        [Fact]
        public void GetCurrentUserValues_ShouldReturnAllValuesAsDictionary()
        {
            // ARRANGE
            _sut.SetCurrentUserValue(_tempTestKey, "Wert1", "A");
            _sut.SetCurrentUserValue(_tempTestKey, "Wert2", 42); // Int
            _sut.SetCurrentUserValue(_tempTestKey, "Wert3", "C");

            // ACT
            Dictionary<string, object> results = _sut.GetCurrentUserValues(_tempTestKey);

            // ASSERT
            results.ShouldNotBeNull();
            results.Count.ShouldBeGreaterThanOrEqualTo(3);

            results["Wert1"].ToString().ShouldBe("A");
            results["Wert2"].ShouldBe(42); // Der Datentyp (Int) muss erhalten bleiben
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Wenn ein Schlüssel nicht existiert, darf die App nicht abstürzen,
        /// sondern muss ein leeres Dictionary zurückliefern.
        /// </summary>
        [Fact]
        public void GetCurrentUserValues_ShouldReturnEmptyDictionary_WhenKeyDoesNotExist()
        {
            // ACT
            var results = _sut.GetCurrentUserValues($@"Software\GhostKey_{Guid.NewGuid()}");

            // ASSERT
            results.ShouldNotBeNull();
            results.ShouldBeEmpty();
        }

        // =========================================================
        // 2. LOCAL MACHINE TESTS (HKLM)
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Das Lesen aus HKLM benötigt keine Adminrechte und muss immer funktionieren.
        /// Wir prüfen das, indem wir einen Schlüssel lesen, der in jedem Windows-System existiert.
        /// </summary>
        [Fact]
        public void GetLocalMachineValue_ShouldReadExistingWindowsKey()
        {
            // ARRANGE
            // Dieser Pfad existiert auf jedem Windows-Rechner
            string winNtPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
            string valueName = "ProductName";

            // ACT
            var result = _sut.GetLocalMachineValue(winNtPath, valueName);

            // ASSERT
            result.ShouldNotBeNull();
            result.ToString().ShouldContain("Windows"); // z.B. "Windows 10 Pro" oder "Windows 11"
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Das Schreiben in HKLM benötigt zwingend Administrator-Rechte.
        /// Der Test prüft, ob entweder der Wert erfolgreich geschrieben wird (wenn als Admin ausgeführt),
        /// oder ob die korrekte Sicherheitsausnahme geworfen wird (wenn als normaler User ausgeführt).
        /// </summary>
        [Fact]
        public void SetLocalMachineValue_ShouldWriteIfAdmin_OrThrowSecurityException()
        {
            // ARRANGE
            string hklmTestKey = $@"SOFTWARE\eBRestarter_HKLM_Test_{Guid.NewGuid()}";

            try
            {
                // ACT
                _sut.SetLocalMachineValue(hklmTestKey, "AdminTest", "Success", RegistryValueKind.String);

                // ASSERT (Falls wir Admin-Rechte haben)
                var result = _sut.GetLocalMachineValue(hklmTestKey, "AdminTest");
                result.ShouldNotBeNull();
                result.ToString().ShouldBe("Success");

                // Cleanup für HKLM (nur möglich, wenn wir Admin sind)
                Registry.LocalMachine.DeleteSubKeyTree(hklmTestKey, false);
            }
            catch (Exception ex)
            {
                // ASSERT (Falls wir KEINE Admin-Rechte haben)
                // Es muss entweder eine UnauthorizedAccessException oder SecurityException sein.
                (ex is UnauthorizedAccessException || ex is SecurityException)
                    .ShouldBeTrue("Erwartete fehlende Berechtigung, da der Test-Runner nicht als Administrator läuft.");
            }
        }

        // =========================================================
        // CLEANUP (wird nach JEDEM Test automatisch ausgeführt)
        // =========================================================
        public void Dispose()
        {
            // Wir löschen den kompletten Sandbox-Ordner aus der Registry des aktuellen Benutzers.
            // So hinterlässt der Unit-Test absolut keine Spuren.
            try
            {
                using var baseKey = Registry.CurrentUser.OpenSubKey("Software", true);
                if (baseKey != null)
                {
                    // Den speziellen Ordner für diesen Testlauf löschen (nur diesen einen!)
                    string keyToDelete = _tempTestKey.Replace(@"Software\", "");
                    baseKey.DeleteSubKeyTree(keyToDelete, false);
                }
            }
            catch
            {
                // Fehler beim Aufräumen ignorieren, um den Test-Erfolg nicht zu verfälschen
            }
        }
    }
}