using eBRestarter.Core.Application.Common.Results;
using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.UseCases;
using eBRestarter.Infrastructure.BehavioralComponents.Validators;
using NSubstitute.ExceptionExtensions;

//using NSubstitute;
//using Shouldly;
//using System;
//using Xunit;
//
//namespace eBRestarter.Tests.Core.Application.UseCases.ConfigureAutoLogon
//{
//    public class ConfigureAutoLogonUseCaseTests
//    {
//        private readonly IWindowsAutoLogonRepository _mockAutoLogonService;
//        private readonly ICredentialValidationPort _mockCredentialUseCase;
//        private readonly ConfigureAutoLogonUseCase _sut;
//
//        public ConfigureAutoLogonUseCaseTests()
//        {
//            _mockAutoLogonService = Substitute.For<IWindowsAutoLogonRepository>();
//            _mockCredentialUseCase = Substitute.For<ICredentialValidationPort>();
//            
//            _sut = new ConfigureAutoLogonUseCase(
//                _mockAutoLogonService,
//                null, // mock WindowsSystemInfoProvider
//                _mockCredentialUseCase,
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
//            _mockAutoLogonService.Received(1).DisableAutoLogon();
//        }
//
//        [Fact]
//        public void Execute_ShouldReturnHelloBlock_WhenWindowsHelloIsActive()
//        {
//            var request = new ConfigureAutoLogonRequest(IsDeactivateAction: false, false, false, "user", "dom", "pass");
//            _mockAutoLogonService.IsPasswordlessAuthEnabled().Returns(true);
//
//            var result = _sut.Execute(request);
//
//            result.IsSuccess.ShouldBeFalse();
//            result.Errors[0].Metadata["Status"].ShouldBe(AutoLogonResultStatus.WindowsHelloBlockActive);
//            result.Errors[0].Message.ShouldContain("Windows Hello");
//
//            _mockCredentialUseCase.DidNotReceive().ValidateCredentials(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
//        }
//
//        [Fact]
//        public void Execute_ShouldEnableAutoLogon_WhenCredentialsAreValid()
//        {
//            var request = new ConfigureAutoLogonRequest(false, false, false, "Admin", "Workgroup", "TopSecret");
//
//            _mockAutoLogonService.IsPasswordlessAuthEnabled().Returns(false);
//            _mockCredentialUseCase
//                .ValidateCredentials("Admin", "Workgroup", "TopSecret")
//                .Returns(true);
//
//            var result = _sut.Execute(request);
//
//            result.IsSuccess.ShouldBeTrue();
//            result.Value.ShouldBe(AutoLogonResultStatus.Activated);
//            _mockAutoLogonService.Received(1).EnableAutoLogon("Admin", "Workgroup", "TopSecret");
//        }
//
//        [Fact]
//        public void Execute_ShouldReturnValidationError_WhenCredentialsAreInvalid()
//        {
//            var request = new ConfigureAutoLogonRequest(false, false, false, "User", "", "WrongPass");
//            _mockCredentialUseCase.ValidateCredentials(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(false);
//
//            var result = _sut.Execute(request);
//
//            result.IsSuccess.ShouldBeFalse();
//            result.Errors[0].Metadata["Status"].ShouldBe(AutoLogonResultStatus.ValidationError);
//            _mockAutoLogonService.DidNotReceive().EnableAutoLogon(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
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
//            _mockCredentialUseCase.ValidateCredentials(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(true);
//            _mockAutoLogonService.EnableAutoLogon(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
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
//            _mockAutoLogonService.DisableAutoLogon().Throws(new Exception("Kritischer Systemfehler"));
//
//            var result = _sut.Execute(request);
//
//            result.IsSuccess.ShouldBeFalse();
//            result.Errors[0].Metadata["Status"].ShouldBe(AutoLogonResultStatus.UnexpectedError);
//            result.Errors[0].Message.ShouldBe("Kritischer Systemfehler");
//        }
//    }
//}








