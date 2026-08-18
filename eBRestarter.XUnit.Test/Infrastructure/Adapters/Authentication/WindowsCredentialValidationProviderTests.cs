using NSubstitute;
using Shouldly;
using System;
using System.DirectoryServices.AccountManagement;
using Xunit;
using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Authentication;
using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Provider.WindowsOS.Authentication;
using NSubstitute.ExceptionExtensions;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Logging;

namespace eBRestarter.XUnit.Test.Infrastructure.Services.Authentication
{
    /// <summary>
    /// Unit tests for <see cref="AdapterWindowsCredentialValidationProvider"/> verifying Windows and Active Directory credential validation.
    /// </summary>
    public class WindowsCredentialValidationProviderTests
    {
        [Fact]
        public void ValidateCredentials_ShouldUseMachineContext_WhenDomainIsLocalComputer()
        {
            // [R]IGHT: Local computer domain uses DirectoryContextScope.Machine
            // Arrange
            var mockAdService = Substitute.For<IOutboundPortActiveDirectoryProvider>();

            mockAdService
                .ValidateCredentials(Arg.Any<DirectoryContextScope>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(true);

            var useCase = new AdapterWindowsCredentialValidationProvider(
                mockAdService,
                new FakeLogger<AdapterWindowsCredentialValidationProvider>());

            string localMachineName = Environment.MachineName;

            // Act
            var result = useCase.ValidateCredentials("TestUser", localMachineName, "Password123");

            // Assert
            result.ShouldBeTrue();

            mockAdService.Received(1).ValidateCredentials(
                DirectoryContextScope.Machine,
                localMachineName,
                "TestUser",
                "Password123");
        }

        [Fact]
        public void ValidateCredentials_ShouldUseDomainContext_WhenDomainIsDifferentFromMachineName()
        {
            // [R]IGHT: Custom domain different from machine name uses DirectoryContextScope.Domain
            // Arrange
            var mockAdService = Substitute.For<IOutboundPortActiveDirectoryProvider>();
            mockAdService
                .ValidateCredentials(Arg.Any<DirectoryContextScope>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(true);

            var useCase = new AdapterWindowsCredentialValidationProvider(
                mockAdService,
                new FakeLogger<AdapterWindowsCredentialValidationProvider>());

            string someDomain = "FIRMEN_DOMAIN_ABC";

            // Act
            var result = useCase.ValidateCredentials("TestUser", someDomain, "Password123");

            // Assert
            result.ShouldBeTrue();

            mockAdService.Received(1).ValidateCredentials(
                DirectoryContextScope.Domain,
                someDomain,
                "TestUser",
                "Password123");
        }

        [Fact]
        public void ValidateCredentials_ShouldThrowInvalidOperationException_WhenServerIsDown()
        {
            // [E]RROR: PrincipalServerDownException is caught and rethrown as InvalidOperationException
            // Arrange
            var mockAdService = Substitute.For<IOutboundPortActiveDirectoryProvider>();

            mockAdService
                .ValidateCredentials(Arg.Any<DirectoryContextScope>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Throws(new PrincipalServerDownException());

            var useCase = new AdapterWindowsCredentialValidationProvider(
                mockAdService,
                new FakeLogger<AdapterWindowsCredentialValidationProvider>());

            // Act
            var exception = Should.Throw<InvalidOperationException>(() =>
            {
                useCase.ValidateCredentials("TestUser", "DOMAIN", "Password");
            });

            // Assert
            exception.Message.ShouldBe("PrincipalServerDown");
        }

        [Fact]
        public void ValidateCredentials_ShouldReturnFalse_OnAnyOtherException()
        {
            // [E]RROR: Unexpected general exception is caught gracefully and returns false
            // Arrange
            var mockAdService = Substitute.For<IOutboundPortActiveDirectoryProvider>();

            mockAdService
                .ValidateCredentials(Arg.Any<DirectoryContextScope>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Throws(new Exception("Unexpected error in Windows subsystem"));

            var useCase = new AdapterWindowsCredentialValidationProvider(
                mockAdService,
                new FakeLogger<AdapterWindowsCredentialValidationProvider>());

            // Act
            var result = useCase.ValidateCredentials("TestUser", "DOMAIN", "Password");

            // Assert
            result.ShouldBeFalse();
        }
    }
}
