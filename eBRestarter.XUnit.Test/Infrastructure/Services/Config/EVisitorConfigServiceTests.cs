using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Domain.Entities;
using eBRestarter.Infrastructure.Services.Config;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using System;
using Xunit;

namespace eBRestarter.XUnit.Test.Infrastructure.Services.Config
{
    /// <summary>
    /// Testet den EVisitorConfigService unter Verwendung eines vollständig gemockten FileSystems.
    /// Keine echten Festplatten-Zugriffe mehr!
    /// </summary>
    public class EVisitorConfigServiceTests
    {
        private readonly Mock<IPathUseCase> _mockPathUseCase;
        private readonly Mock<IWindowsFileSystemService> _mockFileSystem;
        private readonly Mock<ILogger<EVisitorConfigService>> _mockLogger;
        private readonly EVisitorConfigService _service;
        private const string FakeFilePath = @"C:\AppData\eBRestarter\config.json";

        public EVisitorConfigServiceTests()
        {
            _mockPathUseCase = new Mock<IPathUseCase>();
            _mockFileSystem = new Mock<IWindowsFileSystemService>();
            _mockLogger = new Mock<ILogger<EVisitorConfigService>>();

            _mockPathUseCase.Setup(p => p.RetrieveConfigFilePath()).Returns(FakeFilePath);

            _service = new EVisitorConfigService(
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
            // Sollte versuchen standardmäßig eine neue Config zu speichern
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

