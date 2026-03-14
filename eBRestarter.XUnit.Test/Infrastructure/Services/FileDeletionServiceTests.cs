using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Infrastructure.Services;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace eBRestarter.XUnit.Test.Infrastructure.Services;

public class FileDeletionServiceTests : IDisposable
{
    private readonly FileDeletionService _sut;
    private readonly string _testDirectory;

    public FileDeletionServiceTests()
    {
        _sut = new FileDeletionService();
        _testDirectory = Path.Combine(Path.GetTempPath(), "FileDeletionServiceTests_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            try { Directory.Delete(_testDirectory, true); } catch { }
        }
    }

    [Fact]
    public async Task CountFilesAsync_ShouldReturnCorrectFileCount()
    {
        // Arrange
        var dir1 = Path.Combine(_testDirectory, "dir1");
        var dir2 = Path.Combine(_testDirectory, "dir2");
        Directory.CreateDirectory(dir1);
        Directory.CreateDirectory(dir2);

        File.WriteAllText(Path.Combine(dir1, "file1.txt"), "test");
        File.WriteAllText(Path.Combine(dir1, "file2.txt"), "test");
        File.WriteAllText(Path.Combine(dir2, "file3.txt"), "test");

        var directories = new List<string> { dir1, dir2, "NonExistentDirectory" };

        // Act
        var count = await _sut.CountFilesAsync(directories);

        // Assert
        count.ShouldBe(3);
    }

    [Fact]
    public async Task DeleteFilesAsync_ShouldDeleteFilesAndDirectories()
    {
        // Arrange
        var dir1 = Path.Combine(_testDirectory, "dir1");
        Directory.CreateDirectory(dir1);
        var file1 = Path.Combine(dir1, "file1.txt");
        var file2 = Path.Combine(dir1, "file2.txt");
        File.WriteAllText(file1, "test");
        File.WriteAllText(file2, "test");

        var directories = new List<string> { dir1 };
        
        var reportedStatuses = new List<string>();
        var reportedValues = new List<int>();
        var statusReporter = new Progress<string>(s => reportedStatuses.Add(s));
        var valueReporter = new Progress<int>(v => reportedValues.Add(v));

        // Act
        await _sut.DeleteFilesAsync(directories, statusReporter, valueReporter, CancellationToken.None);

        // Assert
        File.Exists(file1).ShouldBeFalse();
        File.Exists(file2).ShouldBeFalse();
        Directory.Exists(dir1).ShouldBeFalse();

        // The final "Abschließe Bereinigung..." should be reported
        reportedStatuses.ShouldContain("Abschließe Bereinigung...");
        reportedValues.ShouldContain(2); // 2 files deleted
    }

    [Fact]
    public async Task DeleteFilesAsync_WhenCancellationRequested_ShouldStopExecution()
    {
        // Arrange
        var dir1 = Path.Combine(_testDirectory, "dir1");
        Directory.CreateDirectory(dir1);
        File.WriteAllText(Path.Combine(dir1, "file1.txt"), "test");
        File.WriteAllText(Path.Combine(dir1, "file2.txt"), "test");

        var directories = new List<string> { dir1 };
        
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        var statusReporter = new Progress<string>();
        var valueReporter = new Progress<int>();

        // Act
        try
        {
            await _sut.DeleteFilesAsync(directories, statusReporter, valueReporter, cts.Token);
        }
        catch (TaskCanceledException)
        {
            // Expected
        }

        // Assert
        // The first file check might pass before the token check, or it returns early.
        // It shouldn't delete everything and the directory should still exist.
        Directory.Exists(dir1).ShouldBeTrue();
    }

    [Fact]
    public void DeleteSingleFile_ShouldDeleteFile()
    {
        // Arrange
        var file = Path.Combine(_testDirectory, "singlefile.txt");
        File.WriteAllText(file, "content");

        // Act
        _sut.DeleteSingleFile(file);

        // Assert
        File.Exists(file).ShouldBeFalse();
    }

    [Fact]
    public void DeleteSingleFile_WhenFileDoesNotExist_ShouldNotThrow()
    {
        // Arrange
        var file = Path.Combine(_testDirectory, "nonexistent.txt");

        // Act & Assert
        Should.NotThrow(() => _sut.DeleteSingleFile(file));
    }
}