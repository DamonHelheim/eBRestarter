using eBRestarter.Core.Application.Ports.Inbound.Providers;
using eBRestarter.Core.Application.Ports.Outbound.Formatters;
using eBRestarter.Core.Application.UseCases;
using eBRestarter.Core.Application.Models.Errors;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Core.Application.Ports.Outbound.Network;
using eBRestarter.Core.Application.Ports.Outbound.Network;
using eBRestarter.Core.Domain.Entities;
using eBRestarter.Infrastructure.Repositories.Config;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using System;
using Xunit;

namespace eBRestarter.XUnit.Test.Infrastructure.Repositories.Config
{
    /// <summary>
    /// Testet den EVRestarterConfigRepository unter Verwendung eines vollst�ndig gemockten FileSystems.
    /// Keine echten Festplatten-Zugriffe mehr!
    /// </summary>
    public class EVisitorConfigRepositoryTests
    {
        private readonly Mock<IOsPathProviderPort> _mockPathUseCase;
        private readonly Mock<IFileSystemPort> _mockFileSystem;
        private readonly Mock<ILogger<EVRestarterConfigRepository>> _mockLogger;
        private readonly EVRestarterConfigRepository _service;
        private const string FakeFilePath = @"C:\AppData\eBRestarter\config.json";

        public EVisitorConfigRepositoryTests()
        {
            _mockPathUseCase = new Mock<IOsPathProviderPort>();
            _mockFileSystem = new Mock<IFileSystemPort>();
            _mockLogger = new Mock<ILogger<EVRestarterConfigRepository>>();

            _mockPathUseCase.Setup(p => p.RetrieveConfigFilePath()).Returns(FakeFilePath);

            _service = new EVRestarterConfigRepository(
                _mockPathUseCase.Object, 
                _mockFileSystem.Object, 
                _mockLogger.Object
            );
        }

        [Fact]
        public void SaveConfig_ShouldWriteJsonToFileSystem()
        {
            // ARRANGE
            var configToSave = new AppConfig();
            configToSave.Settings.ApiKey = "PlaintextKey123";

            string capturedJson = null!;
            _mockFileSystem
                .Setup(f => f.WriteAllText(FakeFilePath, It.IsAny<string>()))
                .Callback<string, string>((path, content) => capturedJson = content);

            _mockFileSystem.Setup(f => f.GetDirectoryName(FakeFilePath)).Returns(@"C:\AppData\eBRestarter");
            _mockFileSystem.Setup(f => f.DirectoryExists(@"C:\AppData\eBRestarter")).Returns(true);

            // ACT
            _service.SaveConfig(configToSave);

            // ASSERT
            _mockFileSystem.Verify(f => f.WriteAllText(FakeFilePath, It.IsAny<string>()), Times.Once);
            capturedJson.ShouldNotBeNull();
            capturedJson.ShouldContain("PlaintextKey123");
        }

        [Fact]
        public void LoadConfig_ShouldCreateAndReturnDefault_WhenFileDoesNotExist()
        {
            // ARRANGE
            _mockFileSystem.Setup(f => f.FileExists(FakeFilePath)).Returns(false);
            _mockFileSystem.Setup(f => f.GetDirectoryName(FakeFilePath)).Returns(@"C:\AppData\eBRestarter");
            _mockFileSystem.Setup(f => f.DirectoryExists(@"C:\AppData\eBRestarter")).Returns(true);

            // ACT
            var result = _service.LoadConfig();

            // ASSERT
            result.ShouldNotBeNull();
            // Sollte versuchen standardm��ig eine neue Config zu speichern
            _mockFileSystem.Verify(f => f.WriteAllText(FakeFilePath, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void LoadConfig_ShouldReturnDefault_WhenJsonIsBroken()
        {
            // ARRANGE
            _mockFileSystem.Setup(f => f.FileExists(FakeFilePath)).Returns(true);
            _mockFileSystem.Setup(f => f.ReadAllText(FakeFilePath)).Returns("Das hier ist kein JSON!");

            // ACT
            var result = _service.LoadConfig();

            // ASSERT
            result.ShouldNotBeNull();
            result.Settings.ApiKey.ShouldBeEmpty();
        }

        [Fact]
        public void ResetConfig_ShouldWriteDefaultConfigToFileSystem()
        {
            // ARRANGE
            _mockFileSystem.Setup(f => f.GetDirectoryName(FakeFilePath)).Returns(@"C:\AppData\eBRestarter");
            _mockFileSystem.Setup(f => f.DirectoryExists(@"C:\AppData\eBRestarter")).Returns(true);

            // ACT
            _service.ResetConfig();

            // ASSERT
            _mockFileSystem.Verify(f => f.WriteAllText(FakeFilePath, It.IsAny<string>()), Times.Once);
        }
    }
}







