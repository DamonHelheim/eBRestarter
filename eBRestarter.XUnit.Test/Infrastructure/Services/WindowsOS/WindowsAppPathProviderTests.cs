using Shouldly;
using System;
using System.IO;
using Xunit;
using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Provider.WindowsOS;
using eBRestarter.Infrastructure.BehavioralComponents.Providers;

namespace eBRestarter.Tests.Infrastructure.Services.WindowsOS
{
    /// <summary>
    /// Unit tests for <see cref="AdapterWindowsAppPathProvider"/> verifying environment folder path resolution.
    /// </summary>
    public class WindowsAppPathProviderTests
    {
        private readonly AdapterWindowsAppPathProvider _sut;

        public WindowsAppPathProviderTests()
        {
            _sut = new AdapterWindowsAppPathProvider();
        }

        [Fact]
        public void GetAppDataDirectory_ShouldReturnCorrectRoamingAppDataPath()
        {
            // [R]IGHT: Resolves roaming AppData directory to valid rooted system path
            // Arrange
            string expectedPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // Act
            string actualPath = _sut.RetrieveAppDataDirectory();

            // Assert
            actualPath.ShouldBe(expectedPath);
            actualPath.ShouldNotBeNullOrWhiteSpace();
            Path.IsPathRooted(actualPath).ShouldBeTrue("The path must be an absolute system path.");
        }

        [Fact]
        public void GetLocalAppDataDirectory_ShouldReturnCorrectLocalAppDataPath()
        {
            // [R]IGHT: Resolves local AppData directory to valid rooted system path
            // Arrange
            string expectedPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            // Act
            string actualPath = _sut.RetrieveLocalAppDataDirectory();

            // Assert
            actualPath.ShouldBe(expectedPath);
            actualPath.ShouldNotBeNullOrWhiteSpace();
            Path.IsPathRooted(actualPath).ShouldBeTrue();
        }

        [Fact]
        public void GetUserProfileDirectory_ShouldReturnCorrectUserProfilePath()
        {
            // [R]IGHT: Resolves current user profile directory to valid rooted system path
            // Arrange
            string expectedPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            // Act
            string actualPath = _sut.RetrieveUserProfileDirectory();

            // Assert
            actualPath.ShouldBe(expectedPath);
            actualPath.ShouldNotBeNullOrWhiteSpace();
            Path.IsPathRooted(actualPath).ShouldBeTrue();
        }

        [Fact]
        public void GetProgramFilesDirectory_ShouldReturnCorrectProgramFilesPath()
        {
            // [R]IGHT: Resolves 64-bit Program Files directory to valid rooted system path
            // Arrange
            string expectedPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            // Act
            string actualPath = _sut.RetrieveProgramFilesDirectory();

            // Assert
            actualPath.ShouldBe(expectedPath);
            actualPath.ShouldNotBeNullOrWhiteSpace();
            Path.IsPathRooted(actualPath).ShouldBeTrue();
        }

        [Fact]
        public void GetProgramFilesX86Directory_ShouldReturnCorrectProgramFilesX86Path()
        {
            // [R]IGHT: Resolves 32-bit Program Files (x86) directory to valid rooted system path
            // Arrange
            string expectedPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            // Act
            string actualPath = _sut.RetrieveProgramFilesX86Directory();

            // Assert
            actualPath.ShouldBe(expectedPath);
            actualPath.ShouldNotBeNullOrWhiteSpace();
            Path.IsPathRooted(actualPath).ShouldBeTrue();
        }
    }
}
