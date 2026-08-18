using NSubstitute;
using Shouldly;
using System.IO;
using System.Text.Json;
using Xunit;
using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Application;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Infrastructure.BehavioralComponents.Repositories.Authentication;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Logging;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Security;

namespace eBRestarter.XUnit.Test.Infrastructure.Services.Authentication
{
    /// <summary>
    /// Unit tests for <see cref="JsonCredentialStoreRepository"/> verifying credential persistence, encryption round-trip, and error handling.
    /// </summary>
    public class JsonCredentialStoreRepositoryTests
    {
        private readonly string _expectedStoragePath = Path.Combine(@"C:\FakeAppData", "Skylar", "eBRestarter", "eBRestarterConfig.json");

        [Fact]
        public void SaveCredentials_ShouldSerializeToJson_AndWriteToFile()
        {
            // [R]IGHT: Serializes credentials to encrypted JSON format and writes to target storage path
            // Arrange
            var mockFileSystem = Substitute.For<IOutboundPortFileSystem>();
            var mockPathProvider = Substitute.For<IOutboundPortAppPathProvider>();

            mockPathProvider.RetrieveLocalAppDataDirectory().Returns(@"C:\FakeAppData");
            mockFileSystem
                .CombinePaths(Arg.Any<string[]>())
                .Returns(ci => Path.Combine(ci.Arg<string[]>()));

            var store = new JsonCredentialStoreRepository(
                CreateRoundTripEncryption(),
                mockFileSystem,
                new FakeLogger<JsonCredentialStoreRepository>(),
                mockPathProvider);
            var credentials = new ApiCredentials("TestUser", "TestKey123");

            string expectedJson = JsonSerializer.Serialize(credentials);

            // Act
            store.SaveCredentials(credentials);

            // Assert
            mockFileSystem.Received(1).WriteAllText(_expectedStoragePath, expectedJson);
        }

        [Fact]
        public void LoadCredentials_ShouldReturnCredentials_WhenFileExistsAndIsValidJson()
        {
            // [R]IGHT: Reads valid JSON from file and decrypts stored API key
            // Arrange
            var mockFileSystem = Substitute.For<IOutboundPortFileSystem>();
            var mockPathProvider = Substitute.For<IOutboundPortAppPathProvider>();

            mockPathProvider.RetrieveLocalAppDataDirectory().Returns(@"C:\FakeAppData");
            mockFileSystem
                .CombinePaths(Arg.Any<string[]>())
                .Returns(ci => Path.Combine(ci.Arg<string[]>()));

            mockFileSystem.FileExists(_expectedStoragePath).Returns(true);

            string fakeJson = "{\"Username\":\"TestUser\",\"ApiKey\":\"TestKey123\"}";
            mockFileSystem.ReadAllText(_expectedStoragePath).Returns(fakeJson);

            var store = new JsonCredentialStoreRepository(
                CreateRoundTripEncryption(),
                mockFileSystem,
                new FakeLogger<JsonCredentialStoreRepository>(),
                mockPathProvider);

            // Act
            var result = store.LoadCredentials();

            // Assert
            result.ShouldNotBeNull();
            result.Username.ShouldBe("TestUser");
            result.ApiKey.ShouldBe("TestKey123");
        }

        [Theory]
        [InlineData(false, null)]
        [InlineData(true, "Kein Gültiges JSON {")]
        public void LoadCredentials_ShouldReturnNull_WhenFileIsMissingOrBroken(bool fileExists, string? fileContent)
        {
            // [B]OUNDARY / [E]RROR: Returns null when credential file does not exist or contains corrupted JSON
            // Arrange
            var mockFileSystem = Substitute.For<IOutboundPortFileSystem>();
            var mockPathProvider = Substitute.For<IOutboundPortAppPathProvider>();

            mockPathProvider.RetrieveLocalAppDataDirectory().Returns(@"C:\FakeAppData");
            mockFileSystem
                .CombinePaths(Arg.Any<string[]>())
                .Returns(ci => Path.Combine(ci.Arg<string[]>()));

            mockFileSystem.FileExists(_expectedStoragePath).Returns(fileExists);

            if (fileContent != null)
            {
                mockFileSystem.ReadAllText(_expectedStoragePath).Returns(fileContent);
            }

            var store = new JsonCredentialStoreRepository(
                CreateRoundTripEncryption(),
                mockFileSystem,
                new FakeLogger<JsonCredentialStoreRepository>(),
                mockPathProvider);

            // Act
            var result = store.LoadCredentials();

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public void ClearCredentials_ShouldDeleteFile_WhenFileExists()
        {
            // [R]IGHT: Deletes credential storage file when file exists
            // Arrange
            var mockFileSystem = Substitute.For<IOutboundPortFileSystem>();
            var mockPathProvider = Substitute.For<IOutboundPortAppPathProvider>();

            mockPathProvider.RetrieveLocalAppDataDirectory().Returns(@"C:\FakeAppData");
            mockFileSystem
                .CombinePaths(Arg.Any<string[]>())
                .Returns(ci => Path.Combine(ci.Arg<string[]>()));

            mockFileSystem.FileExists(_expectedStoragePath).Returns(true);

            var store = new JsonCredentialStoreRepository(
                CreateRoundTripEncryption(),
                mockFileSystem,
                new FakeLogger<JsonCredentialStoreRepository>(),
                mockPathProvider);

            // Act
            store.ClearCredentials();

            // Assert
            mockFileSystem.Received(1).DeleteFile(_expectedStoragePath);
        }

        [Fact]
        public void ImportFromLegacyFile_ShouldReadBinaryFileCorrectly()
        {
            // [R]IGHT: Deserializes username and key from valid legacy binary credential file
            // Arrange
            var mockFileSystem = Substitute.For<IOutboundPortFileSystem>();
            var mockPathProvider = Substitute.For<IOutboundPortAppPathProvider>();
            
            mockPathProvider.RetrieveLocalAppDataDirectory().Returns(@"C:\FakeAppData");
            mockFileSystem
                .CombinePaths(Arg.Any<string[]>())
                .Returns(ci => Path.Combine(ci.Arg<string[]>()));

            var store = new JsonCredentialStoreRepository(
                CreateRoundTripEncryption(),
                mockFileSystem,
                new FakeLogger<JsonCredentialStoreRepository>(),
                mockPathProvider);

            var ms = new MemoryStream();
            using (var writer = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true))
            {
                writer.Write("LegacyUser");
                writer.Write("LegacyKey999");
            }
            ms.Position = 0;

            mockFileSystem.FileExists("legacyPath").Returns(true);
            mockFileSystem.OpenRead("legacyPath").Returns(ms);

            // Act
            var result = store.ImportFromLegacyFile("legacyPath");

            // Assert
            result.ShouldNotBeNull();
            result.Username.ShouldBe("LegacyUser");
            result.ApiKey.ShouldBe("LegacyKey999");
        }

        [Fact]
        public void ImportFromLegacyFile_ShouldReturnNull_WhenFileDoesNotExist()
        {
            // [E]RROR / [B]OUNDARY: Returns null when legacy credential file is not found on disk
            // Arrange
            var mockFileSystem = Substitute.For<IOutboundPortFileSystem>();
            var mockPathProvider = Substitute.For<IOutboundPortAppPathProvider>();

            mockPathProvider.RetrieveLocalAppDataDirectory().Returns(@"C:\FakeAppData");
            mockFileSystem
                .CombinePaths(Arg.Any<string[]>())
                .Returns(ci => Path.Combine(ci.Arg<string[]>()));

            mockFileSystem.FileExists("nonExistentPath").Returns(false);

            var store = new JsonCredentialStoreRepository(
                CreateRoundTripEncryption(),
                mockFileSystem,
                new FakeLogger<JsonCredentialStoreRepository>(),
                mockPathProvider);

            // Act
            var result = store.ImportFromLegacyFile("nonExistentPath");

            // Assert
            result.ShouldBeNull();
        }
    
        /// <summary>
        /// Creates an encryption test double that returns plaintext strings unmodified.
        /// </summary>
        /// <remarks>
        /// A default substitute returns <see langword="null"/> for <c>Encrypt</c>, which triggers
        /// an <see cref="InvalidOperationException"/> in <c>SaveCredentials</c> (fail-safe security behavior).
        /// For persistence tests, a round-trip mock is the appropriate double.
        /// </remarks>
        private static IOutboundPortEncryption CreateRoundTripEncryption()
        {
            var encryption = Substitute.For<IOutboundPortEncryption>();
            encryption.Encrypt(Arg.Any<string>()).Returns(ci => ci.Arg<string>());
            encryption.Decrypt(Arg.Any<string>()).Returns(ci => ci.Arg<string>());

            return encryption;
        }
    }
}
