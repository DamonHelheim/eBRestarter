using Microsoft.Win32;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Security;
using Xunit;
using eBRestarter.Core.Application.ObjectArchetypes.Constants;
using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Repositories.WindowsOS;

namespace eBRestarter.Tests.Infrastructure.Services.WindowsOS
{
    /// <summary>
    /// Integration tests for <see cref="AdapterWindowsRegistryRepository"/> verifying HKCU user values, HKLM system values, and sandbox key isolation.
    /// </summary>
    public class WindowsRegistryRepositoryTests : IDisposable
    {
        private readonly AdapterWindowsRegistryRepository _sut;
        private readonly string _tempTestKey;

        public WindowsRegistryRepositoryTests()
        {
            _sut = new AdapterWindowsRegistryRepository();
            _tempTestKey = $@"Software\eBRestarter_TestSandbox_{Guid.NewGuid()}";
        }

        [Fact]
        public void SetAndGetCurrentUserValue_ShouldWriteAndReadCorrectly()
        {
            // [R]IGHT: Writes and reads string value under HKCU sandbox key
            // Arrange
            string valueName = "TestString";
            string expectedValue = "Hello Registry!";

            // Act
            _sut.SetUserValue(_tempTestKey, valueName, expectedValue);
            var actualValue = _sut.GetUserValue(_tempTestKey, valueName);

            // Assert
            actualValue.ShouldNotBeNull();
            actualValue.ToString().ShouldBe(expectedValue);
        }

        [Fact]
        public void DeleteUserValue_ShouldRemoveValue_WithoutCrashing()
        {
            // [R]IGHT / [B]OUNDARY: Deletes existing value and does not throw when deleting missing value
            // Arrange
            string valueName = "DeleteMe";
            _sut.SetUserValue(_tempTestKey, valueName, "Trash");

            // Act
            _sut.DeleteUserValue(_tempTestKey, valueName);
            var resultAfterDelete = _sut.GetUserValue(_tempTestKey, valueName);

            // Assert
            resultAfterDelete.ShouldBeNull();
            Should.NotThrow(() => _sut.DeleteUserValue(_tempTestKey, valueName));
        }

        [Fact]
        public void GetCurrentUserValues_ShouldReturnAllValuesAsDictionary()
        {
            // [R]IGHT: Returns all key values as strongly typed dictionary
            // Arrange
            _sut.SetUserValue(_tempTestKey, "Wert1", "A");
            _sut.SetUserValue(_tempTestKey, "Wert2", 42);
            _sut.SetUserValue(_tempTestKey, "Wert3", "C");

            // Act
            Dictionary<string, object> results = _sut.GetUserValues(_tempTestKey);

            // Assert
            results.ShouldNotBeNull();
            results.Count.ShouldBeGreaterThanOrEqualTo(3);

            results["Wert1"].ToString().ShouldBe("A");
            results["Wert2"].ShouldBe(42);
        }

        [Fact]
        public void GetCurrentUserValues_ShouldReturnEmptyDictionary_WhenKeyDoesNotExist()
        {
            // [B]OUNDARY: Returns empty dictionary when registry key does not exist
            // Act
            var results = _sut.GetUserValues($@"Software\GhostKey_{Guid.NewGuid()}");

            // Assert
            results.ShouldNotBeNull();
            results.ShouldBeEmpty();
        }

        [Fact]
        public void GetLocalMachineValue_ShouldReadExistingWindowsKey()
        {
            // [R]IGHT: Reads existing Windows product name string from HKLM CurrentVersion key
            // Arrange
            string winNtPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
            string valueName = "ProductName";

            // Act
            var result = _sut.GetSystemValue(winNtPath, valueName);

            // Assert
            result.ShouldNotBeNull();
            string? productName = result.ToString();
            productName.ShouldNotBeNullOrWhiteSpace();
            productName!.ShouldContain("Windows");
        }

        [Fact]
        public void SetSystemValue_ShouldWriteIfAdmin_OrThrowSecurityException()
        {
            // [R]IGHT / [E]RROR: Writes system value when running with elevated privileges or throws security exception
            // Arrange
            string hklmTestKey = $@"SOFTWARE\eBRestarter_HKLM_Test_{Guid.NewGuid()}";

            try
            {
                // Act
                _sut.SetSystemValue(hklmTestKey, "AdminTest", "Success");

                // Assert
                var result = _sut.GetSystemValue(hklmTestKey, "AdminTest");
                result.ShouldNotBeNull();
                result.ToString().ShouldBe("Success");

                Registry.LocalMachine.DeleteSubKeyTree(hklmTestKey, false);
            }
            catch (Exception ex)
            {
                // Assert
                (ex is UnauthorizedAccessException || ex is SecurityException)
                    .ShouldBeTrue("Expected access restriction exception when running without elevated administrator privileges.");
            }
        }

        /// <summary>
        /// Deletes the sandbox registry key hierarchy created under HKCU for test execution.
        /// </summary>
        public void Dispose()
        {
            try
            {
                using var baseKey = Registry.CurrentUser.OpenSubKey("Software", true);
                if (baseKey != null)
                {
                    string keyToDelete = _tempTestKey.Replace(@"Software\", "");
                    baseKey.DeleteSubKeyTree(keyToDelete, false);
                }
            }
            catch
            {
                // Ignore teardown errors to prevent test pollution
            }

            GC.SuppressFinalize(this);
        }
    }
}
