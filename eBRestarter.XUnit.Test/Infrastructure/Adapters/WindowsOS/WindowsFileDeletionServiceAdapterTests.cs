using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Service.WindowsOS;
using eBRestarter.Tests.TestDoubles;

namespace eBRestarter.Tests.Infrastructure.Handlers
{
    /// <summary>
    /// Unit and integration tests for <see cref="AdapterWindowsFileDeletionService"/> verifying file deletion, exclusion rules, progress reporting, and cancellation.
    /// </summary>
    public class WindowsFileDeletionServiceAdapterTests : IDisposable
    {
        private readonly AdapterWindowsFileDeletionService _sut;
        private readonly string _baseTestDir;

        public WindowsFileDeletionServiceAdapterTests()
        {
            _sut = new AdapterWindowsFileDeletionService(Microsoft.Extensions.Logging.Abstractions.NullLogger<AdapterWindowsFileDeletionService>.Instance);
            _baseTestDir = Path.Combine(Path.GetTempPath(), $"FileDeletionTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(_baseTestDir);
        }

        [Fact]
        public async Task DeleteFilesAsync_ShouldCleanUpEverything_ExceptExclusions()
        {
            // [R]IGHT / [B]OUNDARY: Deletes files recursively while preserving excluded patterns and reporting progress
            // Arrange
            string subDir = Path.Combine(_baseTestDir, "ToBeDeleted");
            Directory.CreateDirectory(subDir);

            string normalFile = Path.Combine(subDir, "normal.txt");
            string protectedFile = Path.Combine(subDir, "moz-extension-data.txt");

            File.WriteAllText(normalFile, "bye");
            File.WriteAllText(protectedFile, "stay");

            var statusUpdates = new List<string>();
            var progressUpdates = new List<int>();

            // SynchronousProgress avoids race conditions with thread-pool dispatches during assertions.
            var statusReporter = new SynchronousProgress<string>(statusUpdates.Add);
            var valueReporter = new SynchronousProgress<int>(progressUpdates.Add);

            // Act
            await _sut.DeleteFilesAsync(
                new List<string> { subDir },
                statusReporter,
                valueReporter,
                CancellationToken.None);

            // Assert
            File.Exists(normalFile).ShouldBeFalse("Standard file should be deleted.");
            File.Exists(protectedFile).ShouldBeTrue("moz-extension file should be preserved.");

            statusUpdates.ShouldContain("Finalizing cleanup operations...");
            progressUpdates.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task DeleteFilesAsync_ShouldStop_WhenCanceled()
        {
            // [B]OUNDARY / [E]RROR: Cancellation token cancels deletion operation immediately
            // Arrange
            for (int i = 0; i < 10; i++)
            {
                File.WriteAllText(Path.Combine(_baseTestDir, $"file{i}.txt"), "data");
            }

            var cts = new CancellationTokenSource();
            var statusReporter = new Progress<string>(_ => { });
            var valueReporter = new Progress<int>(_ => { });

            // Act
            cts.Cancel();

            var exception = await Record.ExceptionAsync(async () =>
                await _sut.DeleteFilesAsync(new List<string> { _baseTestDir }, statusReporter, valueReporter, cts.Token)
            );

            // Assert
            exception.ShouldBeOfType<TaskCanceledException>();
            Directory.GetFiles(_baseTestDir).Length.ShouldBe(10);
        }

        [Fact]
        public void DeleteSingleFile_ShouldWork_IfFileExists()
        {
            // [R]IGHT: Deletes single file when file exists
            // Arrange
            string path = Path.Combine(_baseTestDir, "single.txt");
            File.WriteAllText(path, "content");

            // Act
            _sut.DeleteSingleFile(path);

            // Assert
            File.Exists(path).ShouldBeFalse();
        }

        [Fact]
        public void DeleteSingleFile_ShouldNotThrow_IfFileDoesNotExist()
        {
            // [B]OUNDARY: Non-existent file path does not throw exception
            // Arrange
            const string nonExistentPath = @"C:\NonExistentFile.xyz";

            // Act & Assert
            Should.NotThrow(() => _sut.DeleteSingleFile(nonExistentPath));
        }

        /// <summary>
        /// Deletes the temporary directory structure created for test execution.
        /// </summary>
        public void Dispose()
        {
            if (Directory.Exists(_baseTestDir))
            {
                try { Directory.Delete(_baseTestDir, true); } catch { }
            }

            GC.SuppressFinalize(this);
        }
    }
}
