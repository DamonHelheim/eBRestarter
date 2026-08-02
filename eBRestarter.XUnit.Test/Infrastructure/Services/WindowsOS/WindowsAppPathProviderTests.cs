using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Provider.WindowsOS;
using Shouldly;
using System;
using System.IO;
using Xunit;

namespace eBRestarter.Tests.Infrastructure.Services.WindowsOS
{
    /// <summary>
    /// Testet den WindowsAppPathProvider.
    /// Da diese Klasse ein reiner Wrapper um System.Environment ist, testen wir hier,
    /// ob die Methoden intern auf die korrekten Windows-Umgebungsvariablen verweisen
    /// und ob die zurückgegebenen Strings gültige, absolute Pfade sind.
    /// </summary>
    public class WindowsAppPathProviderTests
    {
        private readonly AdapterWindowsAppPathProvider _sut;

        public WindowsAppPathProviderTests()
        {
            _sut = new AdapterWindowsAppPathProvider();
        }
        // 1. APPDATA / LOCALAPPDATA TESTS

        /// <summary>
        /// Stellt sicher, dass RetrieveAppDataDirectory den korrekten Pfad zum Roaming-AppData Ordner liefert.
        ///
        /// WAS WIRD GETESTET?
        /// Wir rufen das echte System via Environment.GetFolderPath auf und vergleichen es mit der
        /// Ausgabe unseres Wrappers. Zudem prüfen wir, ob es sich um einen validen absoluten Pfad handelt.
        /// </summary>
        [Fact]
        public void GetAppDataDirectory_ShouldReturnCorrectRoamingAppDataPath()
        {
            // ARRANGE
            string expectedPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // ACT
            string actualPath = _sut.RetrieveAppDataDirectory();

            // ASSERT
            actualPath.ShouldBe(expectedPath);
            actualPath.ShouldNotBeNullOrWhiteSpace();
            Path.IsPathRooted(actualPath).ShouldBeTrue("Der Pfad muss ein absoluter Systempfad (z.B. C:\\...) sein.");
        }

        /// <summary>
        /// Stellt sicher, dass RetrieveLocalAppDataDirectory auf den lokalen AppData Ordner verweist.
        /// </summary>
        [Fact]
        public void GetLocalAppDataDirectory_ShouldReturnCorrectLocalAppDataPath()
        {
            // ARRANGE
            string expectedPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            // ACT
            string actualPath = _sut.RetrieveLocalAppDataDirectory();

            // ASSERT
            actualPath.ShouldBe(expectedPath);
            actualPath.ShouldNotBeNullOrWhiteSpace();
            Path.IsPathRooted(actualPath).ShouldBeTrue();
        }
        // 2. USER PROFILE TEST

        /// <summary>
        /// Stellt sicher, dass der Wrapper den korrekten Hauptordner des aktuellen Benutzers findet.
        /// </summary>
        [Fact]
        public void GetUserProfileDirectory_ShouldReturnCorrectUserProfilePath()
        {
            // ARRANGE
            string expectedPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            // ACT
            string actualPath = _sut.RetrieveUserProfileDirectory();

            // ASSERT
            actualPath.ShouldBe(expectedPath);
            actualPath.ShouldNotBeNullOrWhiteSpace();
            Path.IsPathRooted(actualPath).ShouldBeTrue();
        }
        // 3. PROGRAM FILES TESTS

        /// <summary>
        /// Stellt sicher, dass der Pfad für 64-Bit (oder allgemeine) Programme korrekt gemappt wird.
        /// </summary>
        [Fact]
        public void GetProgramFilesDirectory_ShouldReturnCorrectProgramFilesPath()
        {
            // ARRANGE
            string expectedPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            // ACT
            string actualPath = _sut.RetrieveProgramFilesDirectory();

            // ASSERT
            actualPath.ShouldBe(expectedPath);
            actualPath.ShouldNotBeNullOrWhiteSpace();
            Path.IsPathRooted(actualPath).ShouldBeTrue();
        }

        /// <summary>
        /// Stellt sicher, dass explizit der x86 (32-Bit) Programme-Ordner referenziert wird.
        /// </summary>
        [Fact]
        public void GetProgramFilesX86Directory_ShouldReturnCorrectProgramFilesX86Path()
        {
            // ARRANGE
            string expectedPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            // ACT
            string actualPath = _sut.RetrieveProgramFilesX86Directory();

            // ASSERT
            actualPath.ShouldBe(expectedPath);
            actualPath.ShouldNotBeNullOrWhiteSpace();
            Path.IsPathRooted(actualPath).ShouldBeTrue();
        }
    }
}

