using eBRestarter.Core.Application.Interfaces.Authentication;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Application.UseCases.ConfigureAutoLogon;
using Moq;
using Shouldly;
using System;
using Xunit;

namespace eBRestarter.Tests.Core.Application.UseCases.ConfigureAutoLogon
{
    /// <summary>
    /// Testet den ConfigureAutoLogonService.
    /// Der Fokus liegt auf der Einhaltung der Geschäftsregeln: Deaktivierung,
    /// Windows-Hello-Check, Validierung der Credentials und Fehlerbehandlung.
    /// </summary>
    public class ConfigureAutoLogonServiceTests
    {
        private readonly Mock<IWindowsAutoLogonService> _mockAutoLogonService;
        private readonly Mock<ICredentialValidationService> _mockCredentialService;
        private readonly ConfigureAutoLogonService _sut;

        public ConfigureAutoLogonServiceTests()
        {
            _mockAutoLogonService = new Mock<IWindowsAutoLogonService>();
            _mockCredentialService = new Mock<ICredentialValidationService>();

            _sut = new ConfigureAutoLogonService(
                _mockAutoLogonService.Object,
                _mockCredentialService.Object);
        }

        // =========================================================
        // 1. DEAKTIVIERUNG
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Wenn der Benutzer AutoLogon ausschalten möchte, muss dies ohne weitere
        /// Prüfungen (wie Windows Hello oder Credentials) sofort geschehen.
        /// </summary>
        [Fact]
        public void Execute_ShouldDisableAutoLogon_WhenDeactivateActionIsTrue()
        {
            // ARRANGE
            var request = new ConfigureAutoLogonRequest(IsDeactivateAction: true);

            // ACT
            var result = _sut.Execute(request);

            // ASSERT
            result.Success.ShouldBeTrue();
            result.Status.ShouldBe(AutoLogonResultStatus.Deactivated);
            _mockAutoLogonService.Verify(s => s.DisableAutoLogon(), Times.Once);
        }

        // =========================================================
        // 2. WINDOWS HELLO CHECK
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Wenn Windows Hello "Passwordless" aktiv ist, ignoriert Windows die
        /// Registry-Passwörter. Der Service muss diesen Zustand erkennen und abbrechen.
        /// </summary>
        [Fact]
        public void Execute_ShouldReturnHelloBlock_WhenWindowsHelloIsActive()
        {
            // ARRANGE
            var request = new ConfigureAutoLogonRequest(IsDeactivateAction: false, "user", "dom", "pass");
            _mockAutoLogonService.Setup(s => s.IsWindowsHelloPasswordlessEnabled()).Returns(true);

            // ACT
            var result = _sut.Execute(request);

            // ASSERT
            result.Success.ShouldBeFalse();
            result.Status.ShouldBe(AutoLogonResultStatus.WindowsHelloBlockActive);
            result.ErrorMessage.ShouldContain("Windows Hello");

            // Wichtig: Credentials dürfen gar nicht erst geprüft werden
            _mockCredentialService.Verify(s => s.ValidateCredentials(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        // =========================================================
        // 3. VALIDIERUNG & AKTIVIERUNG
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Bevor AutoLogon aktiviert wird, müssen die Anmeldedaten gegen das System
        /// (lokal oder Domain) geprüft werden. Nur bei Erfolg darf aktiviert werden.
        /// </summary>
        [Fact]
        public void Execute_ShouldEnableAutoLogon_WhenCredentialsAreValid()
        {
            // ARRANGE
            var request = new ConfigureAutoLogonRequest(false, "Admin", "Workgroup", "TopSecret");

            _mockAutoLogonService.Setup(s => s.IsWindowsHelloPasswordlessEnabled()).Returns(false);
            _mockCredentialService
                .Setup(s => s.ValidateCredentials("Admin", "Workgroup", "TopSecret"))
                .Returns(true);

            // ACT
            var result = _sut.Execute(request);

            // ASSERT
            result.Success.ShouldBeTrue();
            result.Status.ShouldBe(AutoLogonResultStatus.Activated);
            _mockAutoLogonService.Verify(s => s.EnableAutoLogon("Admin", "Workgroup", "TopSecret"), Times.Once);
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Wenn das Passwort falsch ist, darf der Service EnableAutoLogon nicht aufrufen.
        /// </summary>
        [Fact]
        public void Execute_ShouldReturnValidationError_WhenCredentialsAreInvalid()
        {
            // ARRANGE
            var request = new ConfigureAutoLogonRequest(false, "User", "", "WrongPass");
            _mockCredentialService.Setup(s => s.ValidateCredentials(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            // ACT
            var result = _sut.Execute(request);

            // ASSERT
            result.Success.ShouldBeFalse();
            result.Status.ShouldBe(AutoLogonResultStatus.ValidationError);
            _mockAutoLogonService.Verify(s => s.EnableAutoLogon(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Wenn Felder fehlen, muss die Validierung fehlschlagen.
        /// </summary>
        [Theory]
        [InlineData("", "pass")]
        [InlineData("user", null)]
        public void Execute_ShouldReturnValidationError_WhenFieldsAreMissing(string? user, string? pass)
        {
            // ARRANGE
            var request = new ConfigureAutoLogonRequest(false, user, "dom", pass);

            // ACT
            var result = _sut.Execute(request);

            // ASSERT
            result.Status.ShouldBe(AutoLogonResultStatus.ValidationError);
        }

        // =========================================================
        // 4. FEHLERBEHANDLUNG
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Eine InvalidOperationException im Windows-Service deutet oft auf
        /// fehlende Registry-Keys oder Domain-Probleme hin.
        /// </summary>
        [Fact]
        public void Execute_ShouldReturnDomainError_OnInvalidOperationException()
        {
            // ARRANGE
            var request = new ConfigureAutoLogonRequest(false, "u", "d", "p");
            _mockCredentialService.Setup(s => s.ValidateCredentials(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _mockAutoLogonService.Setup(s => s.EnableAutoLogon(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                                 .Throws(new InvalidOperationException());

            // ACT
            var result = _sut.Execute(request);

            // ASSERT
            result.Status.ShouldBe(AutoLogonResultStatus.DomainError);
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Unbekannte Fehler (z.B. Hardwarefehler, Zugriffsverletzungen) müssen
        /// abgefangen und die Fehlermeldung in der Response zurückgegeben werden.
        /// </summary>
        [Fact]
        public void Execute_ShouldReturnUnexpectedError_OnGeneralException()
        {
            // ARRANGE
            var request = new ConfigureAutoLogonRequest(IsDeactivateAction: true);
            _mockAutoLogonService.Setup(s => s.DisableAutoLogon()).Throws(new Exception("Kritischer Systemfehler"));

            // ACT
            var result = _sut.Execute(request);

            // ASSERT
            result.Status.ShouldBe(AutoLogonResultStatus.UnexpectedError);
            result.ErrorMessage.ShouldBe("Kritischer Systemfehler");
        }
    }
}