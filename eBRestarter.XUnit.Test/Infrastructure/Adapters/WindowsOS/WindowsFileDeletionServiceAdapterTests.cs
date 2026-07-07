using eBRestarter.Infrastructure.Adapters.WindowsOS;

using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace eBRestarter.Tests.Infrastructure.Handlers
{
    /// <summary>
    /// Testet den WindowsFileDeletionAdapter.
    /// Da der Use Case physische Dateien lÃ¶scht, arbeiten wir in einem temporÃ¤ren
    /// Verzeichnis, um die echte LÃ¶schlogik und die Fortschrittsmeldung zu validieren.
    /// </summary>
    public class WindowsFileDeletionServiceAdapterTests : IDisposable
    {
        private readonly WindowsFileDeletionServiceAdapter _sut;
        private readonly string _baseTestDir;

        public WindowsFileDeletionServiceAdapterTests()
        {
            _sut = new WindowsFileDeletionServiceAdapter(Microsoft.Extensions.Logging.Abstractions.NullLogger<WindowsFileDeletionServiceAdapter>.Instance);
            _baseTestDir = Path.Combine(Path.GetTempPath(), $"FileDeletionTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(_baseTestDir);
        }

        // 1. COUNT FILES TESTS

        /// <summary>
        /// Vor dem LÃ¶schen muss die Gesamtzahl der Dateien ermittelt werden,
        /// um den Fortschrittsbalken zu initialisieren.
        /// </summary>
        [Fact]
        public async Task CountFilesAsync_ShouldReturnCorrectNumberOfFiles_IncludingSubdirectories()
        {
            // ARRANGE
            string subDir = Path.Combine(_baseTestDir, "Sub");
            Directory.CreateDirectory(subDir);

            File.WriteAllText(Path.Combine(_baseTestDir, "file1.txt"), "test");
            File.WriteAllText(Path.Combine(_baseTestDir, "file2.txt"), "test");
            File.WriteAllText(Path.Combine(subDir, "file3.txt"), "test");

            var dirs = new List<string> { _baseTestDir };

            // ACT
            int count = await _sut.CountFilesAsync(dirs);

            // ASSERT
            count.ShouldBe(3);
        }

        // 2. DELETE FILES (ASYNC & PROGRESS) TESTS

        /// <summary>
        /// Dies ist der Hauptprozess. Wir mÃ¼ssen sicherstellen, dass:
        /// 1. Dateien wirklich gelÃ¶scht werden.
        /// 2. Verzeichnisse danach entfernt werden.
        /// 3. Fortschrittsberichte (Progress) gesendet werden.
        /// 4. "moz-extension" Dateien wie gewÃ¼nscht ignoriert (nicht gelÃ¶scht) werden.
        /// </summary>
        [Fact]
        public async Task DeleteFilesAsync_ShouldCleanUpEverything_ExceptExclusions()
        {
            // ARRANGE
            string subDir = Path.Combine(_baseTestDir, "ToBeDeleted");
            Directory.CreateDirectory(subDir);

            string normalFile = Path.Combine(subDir, "normal.txt");
            string protectedFile = Path.Combine(subDir, "moz-extension-data.txt");

            File.WriteAllText(normalFile, "bye");
            File.WriteAllText(protectedFile, "stay");

            var statusUpdates = new List<string>();
            var progressUpdates = new List<int>();

            var statusReporter = new Progress<string>(s => statusUpdates.Add(s));
            var valueReporter = new Progress<int>(v => progressUpdates.Add(v));

            // ACT
            await _sut.DeleteFilesAsync(
                new List<string> { subDir },
                statusReporter,
                valueReporter,
                CancellationToken.None);

            // ASSERT
            File.Exists(normalFile).ShouldBeFalse("Normale Datei sollte gelÃ¶scht sein.");
            File.Exists(protectedFile).ShouldBeTrue("moz-extension Datei sollte ignoriert worden sein.");

            // Da das Verzeichnis nicht leer war (protectedFile blieb Ã¼brig),
            // sollte Directory.Delete(dir, true) im catch landen oder fehlschlagen,
            // je nachdem wie robust die Implementierung ist.

            // Check Progress
            statusUpdates.ShouldContain("Finalizing cleanup operations...");
            progressUpdates.ShouldNotBeEmpty();
        }

        /// <summary>
        /// Wenn der Benutzer auf "Abbrechen" klickt, muss der LÃ¶schvorgang sofort stoppen.
        /// </summary>
        [Fact]
        public async Task DeleteFilesAsync_ShouldStop_WhenCanceled()
        {
            // ARRANGE
            for (int i = 0; i < 10; i++)
            {
                File.WriteAllText(Path.Combine(_baseTestDir, $"file{i}.txt"), "data");
            }

            var cts = new CancellationTokenSource();
            var statusReporter = new Progress<string>(_ => { });
            var valueReporter = new Progress<int>(_ => { });

            // ACT
            cts.Cancel(); // Wir brechen ab, BEVOR die Task startet

            // Da Task.Run das Token prÃ¼ft und bei Abbruch wirft, fangen wir das hier ab
            var exception = await Record.ExceptionAsync(async () =>
                await _sut.DeleteFilesAsync(new List<string> { _baseTestDir }, statusReporter, valueReporter, cts.Token)
            );

            // ASSERT
            exception.ShouldBeOfType<TaskCanceledException>();
            // Da sofort abgebrochen wurde, mÃ¼ssen alle Dateien noch da sein
            Directory.GetFiles(_baseTestDir).Length.ShouldBe(10);
        }

        // 3. SINGLE FILE DELETION

        /// <summary>
        /// Testet die einfache, synchrone LÃ¶schung einer einzelnen Datei.
        /// </summary>
        [Fact]
        public void DeleteSingleFile_ShouldWork_IfFileExists()
        {
            // ARRANGE
            string path = Path.Combine(_baseTestDir, "single.txt");
            File.WriteAllText(path, "content");

            // ACT
            _sut.DeleteSingleFile(path);

            // ASSERT
            File.Exists(path).ShouldBeFalse();
        }

        /// <summary>
        /// Die Methode darf keine Exception werfen, wenn die Datei gar nicht existiert.
        /// </summary>
        [Fact]
        public void DeleteSingleFile_ShouldNotThrow_IfFileDoesNotExist()
        {
            // ACT & ASSERT
            Should.NotThrow(() => _sut.DeleteSingleFile("C:\\NonExistentFile.xyz"));
        }

        // CLEANUP
        public void Dispose()
        {
            if (Directory.Exists(_baseTestDir))
            {
                try { Directory.Delete(_baseTestDir, true); } catch { }
            }
        }
    }
}


