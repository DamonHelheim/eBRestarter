using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using System;
using Xunit;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Security;
using eBRestarter.Core.Domain.ValueObjects;
using eBRestarter.Infrastructure.BehavioralComponents.Repositories.Config;
using Microsoft.Extensions.Logging.Testing;

namespace eBRestarter.XUnit.Test.Infrastructure.Repositories.Config
{
    /// <summary>
    /// Unit tests for <see cref="EncryptedEVisitorConfigRepositoryDecorator"/> verifying transparent API key encryption, decryption, and error handling.
    /// </summary>
    public class EncryptedEVisitorConfigRepositoryDecoratorTests
    {
        private readonly IOutboundPortEVisitorConfigRepository _mockInnerService;
        private readonly IOutboundPortEncryption _mockEncryptionUseCase;
        private readonly FakeLogger<EncryptedEVisitorConfigRepositoryDecorator> _mockLogger;
        private readonly EncryptedEVisitorConfigRepositoryDecorator _decorator;

        public EncryptedEVisitorConfigRepositoryDecoratorTests()
        {
            _mockInnerService = Substitute.For<IOutboundPortEVisitorConfigRepository>();
            _mockEncryptionUseCase = Substitute.For<IOutboundPortEncryption>();
            _mockLogger = new FakeLogger<EncryptedEVisitorConfigRepositoryDecorator>();

            _decorator = new EncryptedEVisitorConfigRepositoryDecorator(
                _mockInnerService,
                _mockEncryptionUseCase,
                _mockLogger
            );
        }

        [Fact]
        public void LoadConfig_ShouldDecryptApiKey_WhenPresent()
        {
            // [R]IGHT: Loads configuration and transparently decrypts the stored API key
            // Arrange
            var rawConfig = new AppConfig();
            rawConfig.Settings.ApiKey = "EncryptedApiKey";

            _mockInnerService.LoadConfig().Returns(rawConfig);
            _mockEncryptionUseCase.Decrypt("EncryptedApiKey").Returns("PlaintextApiKey");

            // Act
            var result = _decorator.LoadConfig();

            // Assert
            result.Settings.ApiKey.ShouldBe("PlaintextApiKey");
        }

        [Fact]
        public void LoadConfig_ShouldHandleDecryptionFailure_Gracefully()
        {
            // [B]OUNDARY / [E]RROR: Handles corrupt ciphertext gracefully by setting the API key to empty
            // Arrange
            var rawConfig = new AppConfig();
            rawConfig.Settings.ApiKey = "CorruptApiKey";

            _mockInnerService.LoadConfig().Returns(rawConfig);
            _mockEncryptionUseCase.Decrypt("CorruptApiKey").Returns(string.Empty);

            // Act
            var result = _decorator.LoadConfig();

            // Assert
            result.Settings.ApiKey.ShouldBeEmpty();
        }

        [Fact]
        public void SaveConfig_ShouldEncryptApiKey_AndForwardToInner()
        {
            // [R]IGHT: Encrypts plaintext API key before forwarding configuration to inner repository
            // Arrange
            var configToSave = new AppConfig();
            configToSave.Settings.ApiKey = "PlaintextApiKey";

            _mockEncryptionUseCase.Encrypt("PlaintextApiKey").Returns("EncryptedApiKey");

            string capturedKey = null!;
            _mockInnerService.When(s => s.SaveConfig(Arg.Any<AppConfig>()))
                .Do(ci => capturedKey = ci.Arg<AppConfig>().Settings.ApiKey);

            // Act
            _decorator.SaveConfig(configToSave);

            // Assert
            _mockInnerService.Received(1).SaveConfig(Arg.Any<AppConfig>());
            capturedKey.ShouldBe("EncryptedApiKey");
            // Plaintext key must be preserved in the in-memory object
            configToSave.Settings.ApiKey.ShouldBe("PlaintextApiKey");
        }

        [Fact]
        public void ResetConfig_ShouldForwardToInner()
        {
            // [R]IGHT: Forwards reset configuration request directly to inner repository
            // Act
            _decorator.ResetConfig();

            // Assert
            _mockInnerService.Received(1).ResetConfig();
        }
    }
}
