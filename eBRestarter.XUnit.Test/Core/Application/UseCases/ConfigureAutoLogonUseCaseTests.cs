//using eBRestarter.Core.Application.Validators;
using eBRestarter.Core.Application.Ports.Outbound.Authentication;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Core.Application.Ports.Outbound.Authentication;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
//using eBRestarter.Core.Application.Providers.OperatingSystem.WindowsOS;
using eBRestarter.Core.Application.Ports.Outbound.Network;
using eBRestarter.Core.Application.Ports.Outbound.Network;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Records;
//using Moq;
//using Shouldly;
//using System;
//using Xunit;
//
//namespace eBRestarter.Tests.Core.Application.UseCases.ConfigureAutoLogon
//{
//    public class ConfigureAutoLogonUseCaseTests
//    {
//        private readonly Mock<IWindowsAutoLogonRepository> _mockAutoLogonService;
//        private readonly Mock<ICredentialValidationPort> _mockCredentialUseCase;
//        private readonly ConfigureAutoLogonUseCase _sut;
//
//        public ConfigureAutoLogonUseCaseTests()
//        {
//            _mockAutoLogonService = new Mock<IWindowsAutoLogonRepository>();
//            _mockCredentialUseCase = new Mock<ICredentialValidationPort>();
//            
//            _sut = new ConfigureAutoLogonUseCase(
//                _mockAutoLogonService.Object,
//                null, // mock WindowsSystemInfoProvider
//                _mockCredentialUseCase.Object,
//                new ConfigureAutoLogonValidator());
//        }
//
//        [Fact]
//        public void Execute_ShouldDisableAutoLogon_WhenDeactivateActionIsTrue()
//        {
//            var request = new ConfigureAutoLogonRequest(IsDeactivateAction: true);
//
//            var result = _sut.Execute(request);
//
//            result.IsSuccess.ShouldBeTrue();
//            result.Value.ShouldBe(AutoLogonResultStatus.Deactivated);
//            _mockAutoLogonService.Verify(s => s.DisableAutoLogon(), Times.Once);
//        }
//
//        [Fact]
//        public void Execute_ShouldReturnHelloBlock_WhenWindowsHelloIsActive()
//        {
//            var request = new ConfigureAutoLogonRequest(IsDeactivateAction: false, false, false, "user", "dom", "pass");
//            _mockAutoLogonService.Setup(s => s.IsPasswordlessAuthEnabled()).Returns(true);
//
//            var result = _sut.Execute(request);
//
//            result.IsSuccess.ShouldBeFalse();
//            result.Errors[0].Metadata["Status"].ShouldBe(AutoLogonResultStatus.WindowsHelloBlockActive);
//            result.Errors[0].Message.ShouldContain("Windows Hello");
//
//            _mockCredentialUseCase.Verify(s => s.ValidateCredentials(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
//        }
//
//        [Fact]
//        public void Execute_ShouldEnableAutoLogon_WhenCredentialsAreValid()
//        {
//            var request = new ConfigureAutoLogonRequest(false, false, false, "Admin", "Workgroup", "TopSecret");
//
//            _mockAutoLogonService.Setup(s => s.IsPasswordlessAuthEnabled()).Returns(false);
//            _mockCredentialUseCase
//                .Setup(s => s.ValidateCredentials("Admin", "Workgroup", "TopSecret"))
//                .Returns(true);
//
//            var result = _sut.Execute(request);
//
//            result.IsSuccess.ShouldBeTrue();
//            result.Value.ShouldBe(AutoLogonResultStatus.Activated);
//            _mockAutoLogonService.Verify(s => s.EnableAutoLogon("Admin", "Workgroup", "TopSecret"), Times.Once);
//        }
//
//        [Fact]
//        public void Execute_ShouldReturnValidationError_WhenCredentialsAreInvalid()
//        {
//            var request = new ConfigureAutoLogonRequest(false, false, false, "User", "", "WrongPass");
//            _mockCredentialUseCase.Setup(s => s.ValidateCredentials(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(false);
//
//            var result = _sut.Execute(request);
//
//            result.IsSuccess.ShouldBeFalse();
//            result.Errors[0].Metadata["Status"].ShouldBe(AutoLogonResultStatus.ValidationError);
//            _mockAutoLogonService.Verify(s => s.EnableAutoLogon(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
//        }
//
//        [Theory]
//        [InlineData("", "pass")]
//        [InlineData("user", null)]
//        public void Execute_ShouldReturnValidationError_WhenFieldsAreMissing(string? user, string? pass)
//        {
//            var request = new ConfigureAutoLogonRequest(false, false, false, user, "dom", pass);
//
//            var result = _sut.Execute(request);
//
//            result.IsSuccess.ShouldBeFalse();
//            result.Errors[0].Metadata["Status"].ShouldBe(AutoLogonResultStatus.ValidationError);
//        }
//
//        [Fact]
//        public void Execute_ShouldReturnDomainError_OnInvalidOperationException()
//        {
//            var request = new ConfigureAutoLogonRequest(false, false, false, "u", "d", "p");
//            _mockCredentialUseCase.Setup(s => s.ValidateCredentials(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);
//            _mockAutoLogonService.Setup(s => s.EnableAutoLogon(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
//                                 .Throws(new InvalidOperationException());
//
//            var result = _sut.Execute(request);
//
//            result.IsSuccess.ShouldBeFalse();
//            result.Errors[0].Metadata["Status"].ShouldBe(AutoLogonResultStatus.DomainError);
//        }
//
//        [Fact]
//        public void Execute_ShouldReturnUnexpectedError_OnGeneralException()
//        {
//            var request = new ConfigureAutoLogonRequest(IsDeactivateAction: true);
//            _mockAutoLogonService.Setup(s => s.DisableAutoLogon()).Throws(new Exception("Kritischer Systemfehler"));
//
//            var result = _sut.Execute(request);
//
//            result.IsSuccess.ShouldBeFalse();
//            result.Errors[0].Metadata["Status"].ShouldBe(AutoLogonResultStatus.UnexpectedError);
//            result.Errors[0].Message.ShouldBe("Kritischer Systemfehler");
//        }
//    }
//}








