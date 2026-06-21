using eBRestarter.Core.Application.Interfaces.Authentication;
using eBRestarter.Infrastructure.Services.Authentication;
using Moq;
using Shouldly;
using System;
using System.DirectoryServices.AccountManagement;
using Xunit;

namespace eBRestarter.XUnit.Test.Infrastructure.Services.Authentication
{
    /// <summary>
    /// Testet die Logik zur Validierung von Windows-/Domain-Anmeldedaten.
    /// Wir prÃ¼fen, ob je nach Domain der richtige Kontext gewÃ¤hlt wird und ob
    /// die speziellen Active-Directory-Exceptions korrekt Ã¼bersetzt werden.
    /// </summary>
    public class PrincipalContextCredentialValidationUseCaseTests
    {
        /// <summary>
        /// Wenn der Nutzer sich lokal am PC anmeldet (Domain = Computername), muss das System
        /// zwingend den ContextType.Machine nutzen, da sonst die Anmeldung fehlschlÃ¤gt.
        ///
        /// WAS WIRD GETESTET?
        /// Wir Ã¼bergeben als Domain den Namen des aktuellen Computers (Environment.MachineName).
        /// Wir prÃ¼fen mit Moq, ob der Wrapper exakt mit ContextType.Machine aufgerufen wurde.
        /// </summary>
        [Fact]
        public void ValidateCredentials_ShouldUseMachineContext_WhenDomainIsLocalComputer()
        {
            // ARRANGE
            var mockAdService = new Mock<IActiveDirectoryUseCase>();

            // Wir sagen dem Mock: Wenn du aufgerufen wirst, antworte mit 'true'.
            mockAdService
                .Setup(ad => ad.ValidateCredentials(It.IsAny<ContextType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var useCase = new CredentialValidationHandler(mockAdService.Object);

            string localMachineName = Environment.MachineName;

            // ACT
            var result = useCase.ValidateCredentials("TestUser", localMachineName, "Password123");

            // ASSERT
            result.ShouldBeTrue();

            // WICHTIGSTER CHECK: Wurde ContextType.Machine an das System Ã¼bergeben?
            mockAdService.Verify(ad => ad.ValidateCredentials(
                ContextType.Machine, // <-- Darauf kommt es an!
                localMachineName,
                "TestUser",
                "Password123"), Times.Once);
        }

        /// <summary>
        /// Wenn sich der Nutzer an einer echten Firmendomain anmeldet (z. B. "MEINEFIRMA"),
        /// muss zwingend ContextType.Domain genutzt werden.
        ///
        /// WAS WIRD GETESTET?
        /// Wir Ã¼bergeben eine beliebige Domain, die nicht der Computername ist.
        /// Wir prÃ¼fen mit Moq, ob der Wrapper mit ContextType.Domain aufgerufen wurde.
        /// </summary>
        [Fact]
        public void ValidateCredentials_ShouldUseDomainContext_WhenDomainIsDifferentFromMachineName()
        {
            // ARRANGE
            var mockAdService = new Mock<IActiveDirectoryUseCase>();
            mockAdService
                .Setup(ad => ad.ValidateCredentials(It.IsAny<ContextType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var useCase = new CredentialValidationHandler(mockAdService.Object);

            string someDomain = "FIRMEN_DOMAIN_ABC";

            // ACT
            var result = useCase.ValidateCredentials("TestUser", someDomain, "Password123");

            // ASSERT
            result.ShouldBeTrue();

            // WICHTIGSTER CHECK: Wurde ContextType.Domain an das System Ã¼bergeben?
            mockAdService.Verify(ad => ad.ValidateCredentials(
                ContextType.Domain, // <-- Darauf kommt es an!
                someDomain,
                "TestUser",
                "Password123"), Times.Once);
        }

        /// <summary>
        /// Wenn der Domain-Controller (Server) der Firma offline ist, wirft die Windows-API
        /// eine 'PrincipalServerDownException'. Deine Klasse soll das fangen und in eine eigene
        /// 'InvalidOperationException' mit dem Text "PrincipalServerDown" Ã¼bersetzen.
        ///
        /// WAS WIRD GETESTET?
        /// Wir zwingen den Mock dazu, genau diese spezifische Exception zu werfen.
        /// Dann prÃ¼fen wir mit Shouldly, ob die Ã¼bersetzte Exception nach auÃŸen dringt.
        /// </summary>
        [Fact]
        public void ValidateCredentials_ShouldThrowInvalidOperationException_WhenServerIsDown()
        {
            // ARRANGE
            var mockAdService = new Mock<IActiveDirectoryUseCase>();

            // Wir simulieren einen Serverausfall
            mockAdService
                .Setup(ad => ad.ValidateCredentials(It.IsAny<ContextType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new PrincipalServerDownException());

            var useCase = new CredentialValidationHandler(mockAdService.Object);

            // ACT & ASSERT
            // Wir fangen die Exception ab und prÃ¼fen ihren Typ und Inhalt
            var exception = Should.Throw<InvalidOperationException>(() =>
            {
                useCase.ValidateCredentials("TestUser", "DOMAIN", "Password");
            });

            exception.Message.ShouldBe("PrincipalServerDown");
        }

        /// <summary>
        /// Jede andere Art von Fehler (z.B. falsches Passwort, Account gesperrt, Netzwerkfehler)
        /// soll gefangen werden und einfach als 'false' (Anmeldung fehlgeschlagen) zurÃ¼ckgegeben werden.
        /// Die App darf nicht abstÃ¼rzen!
        ///
        /// WAS WIRD GETESTET?
        /// Wir lassen den Mock eine allgemeine Exception werfen und prÃ¼fen,
        /// ob der Try-Catch-Block hÃ¤lt und 'false' zurÃ¼ckkommt.
        /// </summary>
        [Fact]
        public void ValidateCredentials_ShouldReturnFalse_OnAnyOtherException()
        {
            // ARRANGE
            var mockAdService = new Mock<IActiveDirectoryUseCase>();

            // Wir simulieren einen x-beliebigen unerwarteten Fehler
            mockAdService
                .Setup(ad => ad.ValidateCredentials(It.IsAny<ContextType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception("Irgendein unerwarteter Fehler im Windows-System"));

            var useCase = new CredentialValidationHandler(mockAdService.Object);

            // ACT
            var result = useCase.ValidateCredentials("TestUser", "DOMAIN", "Password");

            // ASSERT
            // Die Exception wurde geschluckt und als Login-Fehlschlag interpretiert
            result.ShouldBeFalse();
        }
    }
}

