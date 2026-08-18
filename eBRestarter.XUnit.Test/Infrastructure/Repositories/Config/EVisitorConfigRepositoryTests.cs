using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using System;
using Xunit;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Core.Domain.ValueObjects;
using eBRestarter.Infrastructure.BehavioralComponents.Repositories.Config;
using Microsoft.Extensions.Logging.Testing;

namespace eBRestarter.XUnit.Test.Infrastructure.Repositories.Config
{
    /// <summary>
    /// Unit tests for <see cref="EVRestarterConfigRepository"/> verifying configuration loading, serialization, fallback creation, and reset functionality using mocked file system operations.
    /// </summary>
    public class EVisitorConfigRepositoryTests
    {
        private readonly IInboundPortOsAppPathProvider _mockPathProvider;
        private readonly IOutboundPortFileSystem _mockFileSystem;
        private readonly FakeLogger<EVRestarterConfigRepository> _mockLogger;
        private readonly EVRestarterConfigRepository _service;
        private const string FakeFilePath = @"C:\AppData\eBRestarter\config.json";

        public EVisitorConfigRepositoryTests()
        {
            _mockPathProvider = Substitute.For<IInboundPortOsAppPathProvider>();
            _mockFileSystem = Substitute.For<IOutboundPortFileSystem>();
            _mockLogger = new FakeLogger<EVRestarterConfigRepository>();

            _mockPathProvider.RetrieveConfigFilePath().Returns(FakeFilePath);

            _service = new EVRestarterConfigRepository(
                _mockPathProvider, 
                _mockFileSystem, 
                _mockLogger
            );
        }

        [Fact]
        public void SaveConfig_ShouldWriteJsonToFileSystem()
        {
            // [R]IGHT: Serializes configuration to JSON and writes payload to the specified file path
            // Arrange
            var configToSave = new AppConfig();
            configToSave.Settings.ApiKey = "PlaintextKey123";

            string capturedJson = null!;
            // Capture the serialized JSON written to disk via NSubstitute argument matchers
            _mockFileSystem
                .When(fs => fs.WriteAllText(FakeFilePath, Arg.Any<string>()))
                .Do(ci => capturedJson = ci.ArgAt<string>(1));

            _mockFileSystem.GetDirectoryName(FakeFilePath).Returns(@"C:\AppData\eBRestarter");
            _mockFileSystem.DirectoryExists(@"C:\AppData\eBRestarter").Returns(true);

            // Act
            _service.SaveConfig(configToSave);

            // Assert
            _mockFileSystem.Received(1).WriteAllText(FakeFilePath, Arg.Any<string>());
            capturedJson.ShouldNotBeNull();
            capturedJson.ShouldContain("PlaintextKey123");
        }

        [Fact]
        public void LoadConfig_ShouldCreateAndReturnDefault_WhenFileDoesNotExist()
        {
            // [B]OUNDARY: Automatically creates, persists, and returns default configuration when file is absent
            // Arrange
            _mockFileSystem.FileExists(FakeFilePath).Returns(false);
            _mockFileSystem.GetDirectoryName(FakeFilePath).Returns(@"C:\AppData\eBRestarter");
            _mockFileSystem.DirectoryExists(@"C:\AppData\eBRestarter").Returns(true);

            // Act
            var result = _service.LoadConfig();

            // Assert
            result.ShouldNotBeNull();
            // Verifies that a default configuration file is automatically created and written to disk
            _mockFileSystem.Received(1).WriteAllText(FakeFilePath, Arg.Any<string>());
        }

        [Fact]
        public void LoadConfig_ShouldReturnDefault_WhenJsonIsBroken()
        {
            // [E]RROR / [B]OUNDARY: Handles malformed JSON gracefully by returning a clean default configuration
            // Arrange
            _mockFileSystem.FileExists(FakeFilePath).Returns(true);
            _mockFileSystem.ReadAllText(FakeFilePath).Returns("Invalid JSON content {");

            // Act
            var result = _service.LoadConfig();

            // Assert
            result.ShouldNotBeNull();
            result.Settings.ApiKey.ShouldBeEmpty();
        }

        [Fact]
        public void ResetConfig_ShouldWriteDefaultConfigToFileSystem()
        {
            // [R]IGHT: Overwrites configuration on file system with newly initialized default values
            // Arrange
            _mockFileSystem.GetDirectoryName(FakeFilePath).Returns(@"C:\AppData\eBRestarter");
            _mockFileSystem.DirectoryExists(@"C:\AppData\eBRestarter").Returns(true);

            // Act
            _service.ResetConfig();

            // Assert
            _mockFileSystem.Received(1).WriteAllText(FakeFilePath, Arg.Any<string>());
        }
    }
}
