using eBRestarter.Infrastructure.Services.WindowsOS;
using Shouldly;
using System;
using System.IO;
using Xunit;

namespace eBRestarter.Tests.Infrastructure.Services.WindowsOS
{
    /// <summary>
    /// Testet den WindowsFileSystemService.
    /// Da diese Klasse die unterste Ebene darstellt und System.IO wrappt,
    /// führen wir hier echte Dateioperationen in einem isolierten, temporären Ordner durch.
    /// </summary>
    public class WindowsFileSystemServiceTests : IDisposable
    {
        private readonly WindowsFileSystemService _sut;
        private readonly string _tempTestDirectory;

        public WindowsFileSystemServiceTests()
        {
            _sut = new WindowsFileSystemService();

            // Für JEDEN Testdurchlauf einen einzigartigen, temporären Ordner erstellen
            // So stören sich parallele Tests nicht gegenseitig.
            _tempTestDirectory = Path.Combine(Path.GetTempPath(), $"eB_FS_Test_{Guid.NewGuid()}");
            Directory.CreateDirectory(_tempTestDirectory);
        }
        // 1. EXISTENCE TESTS (File & Directory)

        /// <summary>
        /// Stellt sicher, dass der Wrapper korrekte Booleans für physisch vorhandene
        /// und nicht vorhandene Dateien zurückgibt.
        ///
        /// WAS WIRD GETESTET?
        /// Wir prüfen eine existierende Datei (wird vorher erstellt) und einen Fantasie-Pfad.
        /// </summary>
        [Fact]
        public void FileExists_ShouldReturnCorrectBoolean()
        {
            // ARRANGE
            string existingFilePath = Path.Combine(_tempTestDirectory, "exists.txt");
            string missingFilePath = Path.Combine(_tempTestDirectory, "missing.txt");
            File.WriteAllText(existingFilePath, "Test");

            // ACT & ASSERT
            _sut.FileExists(existingFilePath).ShouldBeTrue();
            _sut.FileExists(missingFilePath).ShouldBeFalse();
        }

        /// <summary>
        /// Stellt sicher, dass Verzeichnisse korrekt erkannt werden.
        /// </summary>
        [Fact]
        public void DirectoryExists_ShouldReturnCorrectBoolean()
        {
            // ARRANGE
            string existingDirPath = Path.Combine(_tempTestDirectory, "SubFolder");
            string missingDirPath = Path.Combine(_tempTestDirectory, "GhostFolder");
            Directory.CreateDirectory(existingDirPath);

            // ACT & ASSERT
            _sut.DirectoryExists(existingDirPath).ShouldBeTrue();
            _sut.DirectoryExists(missingDirPath).ShouldBeFalse();
        }
        // 2. PATH & ENVIRONMENT TESTS

        /// <summary>
        /// CombinePaths muss Arrays von Strings plattformkonform verbinden.
        /// </summary>
        [Fact]
        public void CombinePaths_ShouldCombineCorrectly()
        {
            // ACT
            // Hinweis: Um sicherzugehen, dass Path.Combine das Laufwerk als absoluten Pfad
            // versteht, übergibt man das Root-Verzeichnis mit Backslash (C:\).
            string result = _sut.CombinePaths(@"C:\", "Ordner", "Datei.txt");

            // ASSERT
            // Wir erwarten exakt einen sauberen Windows-Pfad.
            result.ShouldBe(@"C:\Ordner\Datei.txt");
        }

        /// <summary>
        /// Der Service hat eine eigene Fallback-Logik für Umgebungsvariablen (wie appdata).
        /// Diese muss korrekt in den Environment.SpecialFolder übersetzt werden.
        ///
        /// WAS WIRD GETESTET?
        /// Wir fragen bekannte Kürzel ("appdata", "programfiles") ab und prüfen,
        /// ob ein gültiger, nicht leerer Pfad zurückkommt.
        /// </summary>
        [Theory]
        [InlineData("appdata")]
        [InlineData("localappdata")]
        [InlineData("programfiles")]
        public void GetEnvironmentPath_ShouldReturnValidPath_ForKnownVariables(string variable)
        {
            // ACT
            string result = _sut.ResolveEnvironmentPath(variable);

            // ASSERT
            result.ShouldNotBeNullOrWhiteSpace();
            Path.IsPathRooted(result).ShouldBeTrue("Der zurückgegebene Pfad muss ein absoluter Systempfad sein.");
        }

        [Fact]
        public void GetEnvironmentPath_ShouldReturnEmptyString_ForUnknownVariables()
        {
            // ACT
            string result = _sut.ResolveEnvironmentPath("GibtsNicht_12345");

            // ASSERT
            result.ShouldBeEmpty();
        }
        // 3. DELETE TESTS

        /// <summary>
        /// Löschen ist eine destruktive Aktion. Wenn der Pfad leer ist, muss sofort abgebrochen
        /// und eine ArgumentException geworfen werden, um unvorhersehbares Verhalten zu vermeiden.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void DeleteFile_ShouldThrowArgumentException_WhenPathIsNullOrWhiteSpace(string invalidPath)
        {
            // ACT
            Action act = () => _sut.DeleteFile(invalidPath);

            // ASSERT
            act.ShouldThrow<ArgumentException>().ParamName.ShouldBe("path");
        }

        /// <summary>
        /// Die Kernfunktion: Die Datei muss danach physisch von der Festplatte verschwunden sein.
        /// </summary>
        [Fact]
        public void DeleteFile_ShouldRemoveFileFromDisk()
        {
            // ARRANGE
            string targetFile = Path.Combine(_tempTestDirectory, "delete_me.txt");
            File.WriteAllText(targetFile, "Trash");

            // Sanity Check: Datei muss vorher existieren
            File.Exists(targetFile).ShouldBeTrue();

            // ACT
            _sut.DeleteFile(targetFile);

            // ASSERT
            File.Exists(targetFile).ShouldBeFalse();
        }
        // 4. READ / WRITE TESTS

        /// <summary>
        /// Stellt sicher, dass das Schreiben und anschließende Lesen von Strings
        /// über den Service ohne Datenverlust funktioniert.
        ///
        /// WAS WIRD GETESTET?
        /// Wir nutzen den Service zum Schreiben und lesen ihn sofort danach wieder aus.
        /// </summary>
        [Fact]
        public void WriteAllText_And_ReadAllText_ShouldWorkCorrectly()
        {
            // ARRANGE
            string filePath = Path.Combine(_tempTestDirectory, "io_test.txt");
            string expectedContent = "Hallo Welt! \n Das ist ein Test.";

            // ACT
            _sut.WriteAllText(filePath, expectedContent);
            string actualContent = _sut.ReadAllText(filePath);

            // ASSERT
            actualContent.ShouldBe(expectedContent);
        }

        /// <summary>
        /// ReadAllLines muss die Datei zeilenweise splitten und als String-Array zurückgeben.
        /// </summary>
        [Fact]
        public void ReadAllLines_ShouldReturnStringArray()
        {
            // ARRANGE
            string filePath = Path.Combine(_tempTestDirectory, "lines_test.txt");
            string[] expectedLines = { "Zeile 1", "Zeile 2", "Zeile 3" };
            File.WriteAllLines(filePath, expectedLines); // Setup via System.IO

            // ACT
            string[] actualLines = _sut.ReadAllLines(filePath);

            // ASSERT
            actualLines.Length.ShouldBe(3);
            actualLines[0].ShouldBe("Zeile 1");
            actualLines[2].ShouldBe("Zeile 3");
        }
        // CLEANUP (wird nach JEDEM Test automatisch ausgeführt)
        public void Dispose()
        {
            // Sicherheits-Cleanup: Den temporären Ordner samt Inhalt löschen
            if (Directory.Exists(_tempTestDirectory))
            {
                try
                {
                    Directory.Delete(_tempTestDirectory, true);
                }
                catch
                {
                    // Im Unit-Test Kontext schlucken wir hier den Fehler,
                    // falls das OS die Datei noch einen Bruchteil einer Sekunde blockiert.
                }
            }
        }
    }
}
