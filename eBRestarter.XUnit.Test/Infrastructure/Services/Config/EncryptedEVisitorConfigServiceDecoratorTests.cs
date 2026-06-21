using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.Security;
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
    /// Testet den EncryptedEVisitorConfigServiceDecorator auf korrekte Ver- und Entschlüsselung.
    /// </summary>
    public class EncryptedEVisitorConfigServiceDecoratorTests
    {
        private readonly Mock<IEVisitorConfigService> _mockInnerService;
        private readonly Mock<IEncryptionUseCase> _mockEncryptionUseCase;
        private readonly Mock<ILogger<EncryptedEVisitorConfigServiceDecorator>> _mockLogger;
        private readonly EncryptedEVisitorConfigServiceDecorator _decorator;

        public EncryptedEVisitorConfigServiceDecoratorTests()
        {
            _mockInnerService = new Mock<IEVisitorConfigService>();
            _mockEncryptionUseCase = new Mock<IEncryptionUseCase>();
            _mockLogger = new Mock<ILogger<EncryptedEVisitorConfigServiceDecorator>>();

            _decorator = new EncryptedEVisitorConfigServiceDecorator(
                _mockInnerService.Object,
                _mockEncryptionUseCase.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public void LoadConfig_ShouldDecryptApiKey_WhenPresent()
        {
            // ARRANGE
            var rawConfig = new AppConfig();
            rawConfig.Settings.ApiKey = "EncryptedApiKey";

            _mockInnerService.Setup(s => s.LoadConfig()).Returns(rawConfig);
            _mockEncryptionUseCase.Setup(e => e.Decrypt("EncryptedApiKey")).Returns("PlaintextApiKey");

            // ACT
            var result = _decorator.LoadConfig();

            // ASSERT
            result.Settings.ApiKey.ShouldBe("PlaintextApiKey");
        }

        [Fact]
        public void LoadConfig_ShouldHandleDecryptionFailure_Gracefully()
        {
            // ARRANGE
            var rawConfig = new AppConfig();
            rawConfig.Settings.ApiKey = "CorruptApiKey";

            _mockInnerService.Setup(s => s.LoadConfig()).Returns(rawConfig);
            _mockEncryptionUseCase.Setup(e => e.Decrypt("CorruptApiKey")).Returns(string.Empty);

            // ACT
            var result = _decorator.LoadConfig();

            // ASSERT
            result.Settings.ApiKey.ShouldBeEmpty();
        }

        [Fact]
        public void SaveConfig_ShouldEncryptApiKey_AndForwardToInner()
        {
            // ARRANGE
            var configToSave = new AppConfig();
            configToSave.Settings.ApiKey = "PlaintextApiKey";

            _mockEncryptionUseCase.Setup(e => e.Encrypt("PlaintextApiKey")).Returns("EncryptedApiKey");

            string capturedKey = null!;
            _mockInnerService
                .Setup(s => s.SaveConfig(It.IsAny<AppConfig>()))
                .Callback<AppConfig>(c => capturedKey = c.Settings.ApiKey);

            // ACT
            _decorator.SaveConfig(configToSave);

            // ASSERT
            _mockInnerService.Verify(s => s.SaveConfig(It.IsAny<AppConfig>()), Times.Once);
            capturedKey.ShouldBe("EncryptedApiKey");
            // Der Klartext-Key muss im Speicher-Objekt erhalten bleiben!
            configToSave.Settings.ApiKey.ShouldBe("PlaintextApiKey");
        }

        [Fact]
        public void ResetConfig_ShouldForwardToInner()
        {
            // ACT
            _decorator.ResetConfig();

            // ASSERT
            _mockInnerService.Verify(s => s.ResetConfig(), Times.Once);
        }
    }
}

