using Shouldly;
using System;
using System.IO;
using Xunit;
using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Wrapper.WindowsOS;

namespace eBRestarter.Tests.Infrastructure.Services.WindowsOS
{
    /// <summary>
    /// Integration tests for <see cref="AdapterWindowsFileSystem"/> verifying file existence, path combining, environment resolution, and I/O operations in an isolated temporary directory.
    /// </summary>
    public class WindowsFileSystemServiceAdapterTests : IDisposable
    {
        private readonly AdapterWindowsFileSystem _sut;
        private readonly string _tempTestDirectory;

        public WindowsFileSystemServiceAdapterTests()
        {
            _sut = new AdapterWindowsFileSystem();

            _tempTestDirectory = Path.Combine(Path.GetTempPath(), $"eB_FS_Test_{Guid.NewGuid()}");
            Directory.CreateDirectory(_tempTestDirectory);
        }

        [Fact]
        public void FileExists_ShouldReturnCorrectBoolean()
        {
            // [R]IGHT / [B]OUNDARY: Returns true for existing files and false for missing paths
            // Arrange
            string existingFilePath = Path.Combine(_tempTestDirectory, "exists.txt");
            string missingFilePath = Path.Combine(_tempTestDirectory, "missing.txt");
            File.WriteAllText(existingFilePath, "Test");

            // Act
            bool exists = _sut.FileExists(existingFilePath);
            bool missing = _sut.FileExists(missingFilePath);

            // Assert
            exists.ShouldBeTrue();
            missing.ShouldBeFalse();
        }

        [Fact]
        public void DirectoryExists_ShouldReturnCorrectBoolean()
        {
            // [R]IGHT / [B]OUNDARY: Returns true for existing directories and false for missing paths
            // Arrange
            string existingDirPath = Path.Combine(_tempTestDirectory, "SubFolder");
            string missingDirPath = Path.Combine(_tempTestDirectory, "GhostFolder");
            Directory.CreateDirectory(existingDirPath);

            // Act
            bool exists = _sut.DirectoryExists(existingDirPath);
            bool missing = _sut.DirectoryExists(missingDirPath);

            // Assert
            exists.ShouldBeTrue();
            missing.ShouldBeFalse();
        }

        [Fact]
        public void CombinePaths_ShouldCombineCorrectly()
        {
            // [R]IGHT: Combines multiple path segments into platform-compliant path string
            // Act
            string result = _sut.CombinePaths(@"C:\", "Ordner", "Datei.txt");

            // Assert
            result.ShouldBe(@"C:\Ordner\Datei.txt");
        }

        [Theory]
        [InlineData("appdata")]
        [InlineData("localappdata")]
        [InlineData("programfiles")]
        public void GetEnvironmentPath_ShouldReturnValidPath_ForKnownVariables(string variable)
        {
            // [R]IGHT: Resolves standard environment variable alias to absolute rooted path
            // Act
            string result = _sut.ResolveEnvironmentPath(variable);

            // Assert
            result.ShouldNotBeNullOrWhiteSpace();
            Path.IsPathRooted(result).ShouldBeTrue("The resolved path must be an absolute system path.");
        }

        [Fact]
        public void GetEnvironmentPath_ShouldReturnEmptyString_ForUnknownVariables()
        {
            // [B]OUNDARY: Returns empty string for unknown environment variable alias
            // Act
            string result = _sut.ResolveEnvironmentPath("GibtsNicht_12345");

            // Assert
            result.ShouldBeEmpty();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void DeleteFile_ShouldThrowArgumentException_WhenPathIsNullOrWhiteSpace(string? invalidPath)
        {
            // [B]OUNDARY / [E]RROR: Null, empty, or whitespace path throws ArgumentException
            // Act
            Action act = () => _sut.DeleteFile(invalidPath!);

            // Assert
            act.ShouldThrow<ArgumentException>().ParamName.ShouldBe("path");
        }

        [Fact]
        public void DeleteFile_ShouldRemoveFileFromDisk()
        {
            // [R]IGHT: Deletes existing file from physical disk
            // Arrange
            string targetFile = Path.Combine(_tempTestDirectory, "delete_me.txt");
            File.WriteAllText(targetFile, "Trash");

            // Act
            _sut.DeleteFile(targetFile);

            // Assert
            File.Exists(targetFile).ShouldBeFalse();
        }

        [Fact]
        public void WriteAllText_And_ReadAllText_ShouldWorkCorrectly()
        {
            // [R]IGHT / [I]NVERSE: Writes content to file and verifies complete text retrieval
            // Arrange
            string filePath = Path.Combine(_tempTestDirectory, "io_test.txt");
            string expectedContent = "Hallo Welt! \n Das ist ein Test.";

            // Act
            _sut.WriteAllText(filePath, expectedContent);
            string actualContent = _sut.ReadAllText(filePath);

            // Assert
            actualContent.ShouldBe(expectedContent);
        }

        [Fact]
        public void ReadAllLines_ShouldReturnStringArray()
        {
            // [R]IGHT: Reads file contents and returns individual lines as string array
            // Arrange
            string filePath = Path.Combine(_tempTestDirectory, "lines_test.txt");
            string[] expectedLines = { "Zeile 1", "Zeile 2", "Zeile 3" };
            File.WriteAllLines(filePath, expectedLines);

            // Act
            string[] actualLines = _sut.ReadAllLines(filePath);

            // Assert
            actualLines.Length.ShouldBe(3);
            actualLines[0].ShouldBe("Zeile 1");
            actualLines[2].ShouldBe("Zeile 3");
        }

        /// <summary>
        /// Deletes the temporary directory structure created for test execution.
        /// </summary>
        public void Dispose()
        {
            if (Directory.Exists(_tempTestDirectory))
            {
                try
                {
                    Directory.Delete(_tempTestDirectory, true);
                }
                catch
                {
                    // Ignore file lock errors during test teardown
                }
            }

            GC.SuppressFinalize(this);
        }
    }
}
