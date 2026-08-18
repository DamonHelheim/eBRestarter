using NSubstitute;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;
using eBRestarter.Core.Application.BehavioralComponents.Providers;
using eBRestarter.Core.Application.Common.Results;
using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using NSubstitute.ExceptionExtensions;

namespace eBRestarter.Tests.Core.Application.UseCases.GetSystemInformation
{
    /// <summary>
    /// Testet den SystemInformationProvider.
    /// Dieser UseCase aggregiert Daten aus drei verschiedenen System-Services.
    /// Wir testen hier, ob das Mapping der Rückgabewerte in die finale Response fehlerfrei funktioniert.
    /// </summary>
    public class SystemInformationProviderTests
    {
        private readonly IOutboundPortHardwareInfoProvider _mockHardwareService;
        private readonly IOutboundPortOsEditionProvider _mockOsEditionService;
        private readonly IOutboundPortSystemInfoProvider _mockSystemInfoService;
        private readonly SystemInformationProvider _sut;

        public SystemInformationProviderTests()
        {
            _mockHardwareService = Substitute.For<IOutboundPortHardwareInfoProvider>();
            _mockOsEditionService = Substitute.For<IOutboundPortOsEditionProvider>();
            _mockSystemInfoService = Substitute.For<IOutboundPortSystemInfoProvider>();

            _sut = new SystemInformationProvider(
                _mockHardwareService,
                _mockOsEditionService,
                _mockSystemInfoService);
        }
        // 1. HAPPY PATH (ERFOLGREICHES MAPPING)

        /// <summary>
        /// Das ist der wichtigste Test. Er stellt sicher, dass alle Eigenschaften
        /// exakt an die richtige Stelle im SystemInformationResponse-Record gemappt werden
        /// und nichts vertauscht wird (z.B. OS-Build mit OS-Version).
        ///
        /// WAS WIRD GETESTET?
        /// Wir zwingen alle Mocks dazu, hochspezifische Fake-Strings zurückzugeben.
        /// Danach prüfen wir, ob die generierte Response genau diese Strings an den
        /// erwarteten Stellen enthält.
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_ShouldAggregateAndMapAllDataCorrectly()
        {
            // ARRANGE

            // 1. Hardware Mock aufbauen
            // (Annahme: Du hast ein Record/Klasse, die die 3 Hardware-Eigenschaften enthält.
            // Falls das Objekt anders heißt, bitte den Typnamen hier kurz anpassen!)
            var fakeHardware = new HardwareInfo
            {
                ProcessorName = "AMD Ryzen 9 5900X",
                GraphicsCardName = "NVIDIA RTX 3080",
                InstalledRam = "32 GB"
            };

            _mockHardwareService.RetrieveHardwareInfoAsync().Returns(fakeHardware);

            // 2. OS Edition Mock
            _mockOsEditionService.RetrieveOsEditionAsync().Returns("Windows 11 Pro");

            // 3. System Info Mock (Sync-Aufrufe)
            _mockSystemInfoService.RetrieveCurrentOsDisplayVersion().Returns("23H2");
            _mockSystemInfoService.RetrieveCurrentOsBuildVersion().Returns("22631.3296");
            _mockSystemInfoService.RetrieveCurrentStandardBrowserName().Returns("Firefox");

            // ACT
            var result = await _sut.RetrieveAsync();

            // ASSERT
            result.ShouldNotBeNull();

            // Prüfen, ob die Werte 1:1 durchgereicht wurden
            result.ProcessorName.ShouldBe("AMD Ryzen 9 5900X");
            result.GraphicsCardName.ShouldBe("NVIDIA RTX 3080");
            result.InstalledRam.ShouldBe("32 GB");

            result.OsEdition.ShouldBe("Windows 11 Pro");
            result.OsDisplayVersion.ShouldBe("23H2");
            result.OsBuildVersion.ShouldBe("22631.3296");
            result.StandardBrowserName.ShouldBe("Firefox");

            // Überprüfen, dass die Services auch wirklich aufgerufen wurden
            await _mockHardwareService.Received(1).RetrieveHardwareInfoAsync();
            await _mockOsEditionService.Received(1).RetrieveOsEditionAsync();
            _mockSystemInfoService.Received(1).RetrieveCurrentOsDisplayVersion();
            _mockSystemInfoService.Received(1).RetrieveCurrentOsBuildVersion();
            _mockSystemInfoService.Received(1).RetrieveCurrentStandardBrowserName();
        }
        // 2. EXCEPTION BUBBLING (FEHLER WEITERREICHEN)

        /// <summary>
        /// Der Service besitzt keinen eigenen try-catch-Block.
        /// Das bedeutet: Wenn einer der unterliegenden Services abstürzt (z.B. WMI Error bei der Hardware),
        /// muss die Exception ungefiltert an den Aufrufer (z.B. das ViewModel) hochblubbern.
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_ShouldBubbleUpExceptions_WhenDependencyThrows()
        {
            // ARRANGE
            // Wir simulieren, dass der Hardware-Service einen Fehler wirft
            _mockHardwareService
                .RetrieveHardwareInfoAsync()
                .Throws(new UnauthorizedAccessException("WMI Access Denied"));

            // ACT & ASSERT
            var exception = await Should.ThrowAsync<UnauthorizedAccessException>(() => _sut.RetrieveAsync());
            exception.Message.ShouldBe("WMI Access Denied");
        }
    }
}







