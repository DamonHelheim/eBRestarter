using eBRestarter.Infrastructure.Adapters.Authentication;
using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Infrastructure.Repositories.Authentication;
using Moq;
using Shouldly;
using System;
using System.DirectoryServices.AccountManagement;
using Xunit;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Authentication;
using eBRestarter.Core.Application.ObjectArchetypes.Enums;

namespace eBRestarter.XUnit.Test.Infrastructure.Services.Authentication
{
    /// <summary>
    /// Testet die Logik zur Validierung von Windows-/Domain-Anmeldedaten.
    /// Wir prüfen, ob je nach Domain der richtige Kontext gewählt wird und ob
    /// die speziellen Active-Directory-Exceptions korrekt übersetzt werden.
    /// </summary>
    public class WindowsCredentialValidationProviderTests
    {
        /// <summary>
        /// Wenn der Nutzer sich lokal am PC anmeldet (Domain = Computername), muss das System
        /// zwingend den DirectoryContextScope.Machine nutzen, da sonst die Anmeldung fehlschlägt.
        ///
        /// WAS WIRD GETESTET?
        /// Wir übergeben als Domain den Namen des aktuellen Computers (Environment.MachineName).
        /// Wir prüfen mit Moq, ob der Wrapper exakt mit DirectoryContextScope.Machine aufgerufen wurde.
        /// </summary>
        [Fact]
        public void ValidateCredentials_ShouldUseMachineContext_WhenDomainIsLocalComputer()
        {
            // ARRANGE
            var mockAdService = new Mock<IOutboundPortActiveDirectoryProvider>();

            // Wir sagen dem Mock: Wenn du aufgerufen wirst, antworte mit 'true'.
            mockAdService
                .Setup(ad => ad.ValidateCredentials(It.IsAny<DirectoryContextScope>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var useCase = new AdapterWindowsCredentialValidationProvider(mockAdService.Object);

            string localMachineName = Environment.MachineName;

            // ACT
            var result = useCase.ValidateCredentials("TestUser", localMachineName, "Password123");

            // ASSERT
            result.ShouldBeTrue();

            // WICHTIGSTER CHECK: Wurde DirectoryContextScope.Machine an das System übergeben?
            mockAdService.Verify(ad => ad.ValidateCredentials(
                DirectoryContextScope.Machine, // <-- Darauf kommt es an!
                localMachineName,
                "TestUser",
                "Password123"), Times.Once);
        }

        /// <summary>
        /// Wenn sich der Nutzer an einer echten Firmendomain anmeldet (z. B. "MEINEFIRMA"),
        /// muss zwingend DirectoryContextScope.Domain genutzt werden.
        ///
        /// WAS WIRD GETESTET?
        /// Wir übergeben eine beliebige Domain, die nicht der Computername ist.
        /// Wir prüfen mit Moq, ob der Wrapper mit DirectoryContextScope.Domain aufgerufen wurde.
        /// </summary>
        [Fact]
        public void ValidateCredentials_ShouldUseDomainContext_WhenDomainIsDifferentFromMachineName()
        {
            // ARRANGE
            var mockAdService = new Mock<IOutboundPortActiveDirectoryProvider>();
            mockAdService
                .Setup(ad => ad.ValidateCredentials(It.IsAny<DirectoryContextScope>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var useCase = new AdapterWindowsCredentialValidationProvider(mockAdService.Object);

            string someDomain = "FIRMEN_DOMAIN_ABC";

            // ACT
            var result = useCase.ValidateCredentials("TestUser", someDomain, "Password123");

            // ASSERT
            result.ShouldBeTrue();

            // WICHTIGSTER CHECK: Wurde DirectoryContextScope.Domain an das System übergeben?
            mockAdService.Verify(ad => ad.ValidateCredentials(
                DirectoryContextScope.Domain, // <-- Darauf kommt es an!
                someDomain,
                "TestUser",
                "Password123"), Times.Once);
        }

        /// <summary>
        /// Wenn der Domain-Controller (Server) der Firma offline ist, wirft die Windows-API
        /// eine 'PrincipalServerDownException'. Deine Klasse soll das fangen und in eine eigene
        /// 'InvalidOperationException' mit the Text "PrincipalServerDown" übersetzen.
        ///
        /// WAS WIRD GETESTET?
        /// Wir zwingen den Mock dazu, genau diese spezifische Exception zu werfen.
        /// Dann prüfen wir mit Shouldly, ob die übersetzte Exception nach außen dringt.
        /// </summary>
        [Fact]
        public void ValidateCredentials_ShouldThrowInvalidOperationException_WhenServerIsDown()
        {
            // ARRANGE
            var mockAdService = new Mock<IOutboundPortActiveDirectoryProvider>();

            // Wir simulieren einen Serverausfall
            mockAdService
                .Setup(ad => ad.ValidateCredentials(It.IsAny<DirectoryContextScope>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new PrincipalServerDownException());

            var useCase = new AdapterWindowsCredentialValidationProvider(mockAdService.Object);

            // ACT & ASSERT
            // Wir fangen die Exception ab und prüfen ihren Typ und Inhalt
            var exception = Should.Throw<InvalidOperationException>(() =>
            {
                useCase.ValidateCredentials("TestUser", "DOMAIN", "Password");
            });

            exception.Message.ShouldBe("PrincipalServerDown");
        }

        /// <summary>
        /// Jede andere Art von Fehler (z.B. falsches Passwort, Account gesperrt, Netzwerkfehler)
        /// soll gefangen werden und einfach als 'false' (Anmeldung fehlgeschlagen) zurückgegeben werden.
        /// Die App darf nicht abstürzen!
        ///
        /// WAS WIRD GETESTET?
        /// Wir lassen den Mock eine allgemeine Exception werfen und prüfen,
        /// ob der Try-Catch-Block hält und 'false' zurückkommt.
        /// </summary>
        [Fact]
        public void ValidateCredentials_ShouldReturnFalse_OnAnyOtherException()
        {
            // ARRANGE
            var mockAdService = new Mock<IOutboundPortActiveDirectoryProvider>();

            // Wir simulieren einen x-beliebigen unerwarteten Fehler
            mockAdService
                .Setup(ad => ad.ValidateCredentials(It.IsAny<DirectoryContextScope>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception("Irgendein unerwarteter Fehler im Windows-System"));

            var useCase = new AdapterWindowsCredentialValidationProvider(mockAdService.Object);

            // ACT
            var result = useCase.ValidateCredentials("TestUser", "DOMAIN", "Password");

            // ASSERT
            // Die Exception wurde geschluckt und als Login-Fehlschlag interpretiert
            result.ShouldBeFalse();
        }
    }
}





