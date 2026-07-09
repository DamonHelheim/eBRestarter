using eBRestarter.Infrastructure.Adapters.Outbound.WindowsOS.Repositories;
using Microsoft.Win32;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Security;
using Xunit;

namespace eBRestarter.Tests.Infrastructure.Services.WindowsOS
{
    /// <summary>
    /// Testet den WindowsRegistryRepository.
    /// Da diese Klasse tief in das System eingreift, arbeiten wir für Schreibtests
    /// in einem sicheren, temporären Sandbox-Schlüssel unter HKEY_CURRENT_USER.
    /// </summary>
    public class WindowsRegistryRepositoryTests : IDisposable
    {
        private readonly AdapterWindowsRegistryRepository _sut;
        private readonly string _tempTestKey;

        public WindowsRegistryRepositoryTests()
        {
            _sut = new AdapterWindowsRegistryRepository();

            // Wir generieren für jeden Testdurchlauf einen einzigartigen, temporären Registry-Key.
            // Das verhindert, dass sich parallele Tests stören oder das Entwickler-System zugemüllt wird.
            _tempTestKey = $@"Software\eBRestarter_TestSandbox_{Guid.NewGuid()}";
        }
        // 1. CURRENT USER TESTS (HKCU) - Schreiben, Lesen, Löschen

        /// <summary>
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
            _sut.SetUserValue(_tempTestKey, valueName, expectedValue);
            var actualValue = _sut.GetUserValue(_tempTestKey, valueName);

            // ASSERT
            actualValue.ShouldNotBeNull();
            actualValue.ToString().ShouldBe(expectedValue);
        }

        /// <summary>
        /// Wenn ein Wert gelöscht wird, darf er danach nicht mehr existieren. Die Methode
        /// darf außerdem nicht abstürzen, wenn der Wert bereits fehlt (throwOnMissingValue: false).
        /// </summary>
        [Fact]
        public void DeleteUserValue_ShouldRemoveValue_WithoutCrashing()
        {
            // ARRANGE
            string valueName = "DeleteMe";
            _sut.SetUserValue(_tempTestKey, valueName, "Trash"); // Vorher anlegen

            // ACT
            _sut.DeleteUserValue(_tempTestKey, valueName);
            var resultAfterDelete = _sut.GetUserValue(_tempTestKey, valueName);

            // ASSERT
            resultAfterDelete.ShouldBeNull();

            // Zweiter Aufruf darf KEINEN Fehler werfen (Test der Robustheit)
            Should.NotThrow(() => _sut.DeleteUserValue(_tempTestKey, valueName));
        }

        /// <summary>
        /// Stellt sicher, dass das Auslesen eines gesamten Registry-Schlüssels
        /// alle darin enthaltenen Werte als Dictionary zurückgibt.
        /// </summary>
        [Fact]
        public void GetCurrentUserValues_ShouldReturnAllValuesAsDictionary()
        {
            // ARRANGE
            _sut.SetUserValue(_tempTestKey, "Wert1", "A");
            _sut.SetUserValue(_tempTestKey, "Wert2", 42); // Int
            _sut.SetUserValue(_tempTestKey, "Wert3", "C");

            // ACT
            Dictionary<string, object> results = _sut.GetUserValues(_tempTestKey);

            // ASSERT
            results.ShouldNotBeNull();
            results.Count.ShouldBeGreaterThanOrEqualTo(3);

            results["Wert1"].ToString().ShouldBe("A");
            results["Wert2"].ShouldBe(42); // Der Datentyp (Int) muss erhalten bleiben
        }

        /// <summary>
        /// Wenn ein Schlüssel nicht existiert, darf die App nicht abstürzen,
        /// sondern muss ein leeres Dictionary zurückliefern.
        /// </summary>
        [Fact]
        public void GetCurrentUserValues_ShouldReturnEmptyDictionary_WhenKeyDoesNotExist()
        {
            // ACT
            var results = _sut.GetUserValues($@"Software\GhostKey_{Guid.NewGuid()}");

            // ASSERT
            results.ShouldNotBeNull();
            results.ShouldBeEmpty();
        }
        // 2. LOCAL MACHINE TESTS (HKLM)

        /// <summary>
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
            var result = _sut.GetSystemValue(winNtPath, valueName);

            // ASSERT
            result.ShouldNotBeNull();
            result.ToString().ShouldContain("Windows"); // z.B. "Windows 10 Pro" oder "Windows 11"
        }

        /// <summary>
        /// Das Schreiben in HKLM benötigt zwingend Administrator-Rechte.
        /// Der Test prüft, ob entweder der Wert erfolgreich geschrieben wird (wenn als Admin ausgeführt),
        /// oder ob die korrekte Sicherheitsausnahme geworfen wird (wenn als normaler User ausgeführt).
        /// </summary>
        [Fact]
        public void SetSystemValue_ShouldWriteIfAdmin_OrThrowSecurityException()
        {
            // ARRANGE
            string hklmTestKey = $@"SOFTWARE\eBRestarter_HKLM_Test_{Guid.NewGuid()}";

            try
            {
                // ACT
                _sut.SetSystemValue(hklmTestKey, "AdminTest", "Success");

                // ASSERT (Falls wir Admin-Rechte haben)
                var result = _sut.GetSystemValue(hklmTestKey, "AdminTest");
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
        // CLEANUP (wird nach JEDEM Test automatisch ausgeführt)
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

