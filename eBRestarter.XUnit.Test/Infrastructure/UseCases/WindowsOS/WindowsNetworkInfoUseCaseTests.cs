using eBRestarter.Core.Application.Ports.Outbound.Providers;
using eBRestarter.Core.Application.Providers;
using eBRestarter.Infrastructure.Adapters.WindowsOS;
using eBRestarter.Core.Application.Ports.Outbound.Application;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Core.Application.Ports.Outbound.Network;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Core.Application.Ports.Outbound.Network;
using eBRestarter.Core.Application.Models.Records;
using Moq;
using Shouldly;
using System.Linq;
using System.Net.NetworkInformation;
using Xunit;

namespace eBRestarter.XUnit.Test.Infrastructure.Services.WindowsOS
{
    /// <summary>
    /// Testet den NetworkInfoProvider.
    /// Durch den Mock des INetworkProviders können wir verschiedene Netzwerkzustände
    /// (Offline, Loopback, ohne Traffic) simulieren, ohne die echte Hardware zu verändern.
    /// </summary>
    public class WindowsNetworkInfoUseCaseTests
    {
        private readonly Mock<INetworkProviderPort> _mockProvider;
        private readonly NetworkInfoProvider _sut;

        public WindowsNetworkInfoUseCaseTests()
        {
            _mockProvider = new Mock<INetworkProviderPort>();
            _sut = new NetworkInfoProvider(_mockProvider.Object);
        }

        // 1. AVAILABILITY TESTS

        /// <summary>
        /// Stellt sicher, dass das Ergebnis des Providers direkt und ohne
        /// Modifikation weitergegeben wird.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsNetworkAvailable_ShouldReturnResultFromProvider(bool isAvailable)
        {
            // ARRANGE
            _mockProvider.Setup(p => p.CheckIsNetworkAvailable()).Returns(isAvailable);

            // ACT
            var result = _sut.IsNetworkAvailable();

            // ASSERT
            result.ShouldBe(isAvailable);
        }

        // 2. INTERFACE FILTERING TESTS

        /// <summary>
        /// Das ist das Herzstück deiner Klasse. Hier prüfen wir alle Filter-Bedingungen
        /// gleichzeitig. Die Methode darf nur Netzwerkkarten durchlassen, die:
        /// - Status = UP haben
        /// - NICHT vom Typ Loopback sind
        /// - Traffic (BytesSent oder BytesReceived > 0) aufweisen.
        ///
        /// WAS WIRD GETESTET?
        /// Wir erzeugen eine Liste aus 5 künstlichen Netzwerkkarten. Nur 2 davon
        /// erfüllen deine Kriterien. Wir erwarten, dass genau diese 2 gemappt und zurückgegeben werden.
        /// </summary>
        [Fact]
        public void GetActiveInterfaces_ShouldFilterOutInvalidInterfaces_AndMapCorrectly()
        {
            // ARRANGE
            var loopbackMock = CreateNicMock("Loopback", OperationalStatus.Up, NetworkInterfaceType.Loopback, sent: 100, received: 100);
            var downMock = CreateNicMock("OfflineCard", OperationalStatus.Down, NetworkInterfaceType.Ethernet, sent: 100, received: 100);
            var noTrafficMock = CreateNicMock("GhostCard", OperationalStatus.Up, NetworkInterfaceType.Ethernet, sent: 0, received: 0);

            // Diese beiden müssen durchkommen
            var validEthMock = CreateNicMock("Valid Ethernet", OperationalStatus.Up, NetworkInterfaceType.Ethernet, sent: 500, received: 1000);
            var validWifiMock = CreateNicMock("Valid WiFi", OperationalStatus.Up, NetworkInterfaceType.Wireless80211, sent: 0, received: 50); // Nur Downloads

            _mockProvider.Setup(p => p.RetrieveAllNetworkInterfaces()).Returns(new[]
            {
                loopbackMock.Object,
                downMock.Object,
                noTrafficMock.Object,
                validEthMock.Object,
                validWifiMock.Object
            });

            // ACT
            // Da 'RetrieveActiveInterfaces' ein IEnumerable mit 'yield return' nutzt,
            // holen wir uns mit .ToList() die tatsächliche Auswertung.
            var result = _sut.RetrieveActiveInterfaces().ToList();

            // ASSERT
            // 1. Es dürfen exakt nur die 2 gültigen Karten übrig bleiben
            result.Count.ShouldBe(2);

            // 2. Mapping-Check Karte 1 (Ethernet)
            var ethStats = result.First(r => r.Name == "Valid Ethernet");
            ethStats.BytesSent.ShouldBe(500);
            ethStats.BytesReceived.ShouldBe(1000);
            ethStats.IsActive.ShouldBeTrue();

            // 3. Mapping-Check Karte 2 (WiFi)
            var wifiStats = result.First(r => r.Name == "Valid WiFi");
            wifiStats.BytesSent.ShouldBe(0);
            wifiStats.BytesReceived.ShouldBe(50);
            wifiStats.IsActive.ShouldBeTrue();
        }

        // HELPER METHODEN

        /// <summary>
        /// Erzeugt künstliche NetworkInterface-Objekte mit spezifischen Werten.
        /// (Moq kann abstrakte .NET Klassen wie NetworkInterface fälschen).
        /// </summary>
        private Mock<NetworkInterface> CreateNicMock(
            string name,
            OperationalStatus status,
            NetworkInterfaceType type,
            long sent,
            long received)
        {
            var mockNic = new Mock<NetworkInterface>();
            mockNic.Setup(n => n.Name).Returns(name);
            mockNic.Setup(n => n.OperationalStatus).Returns(status);
            mockNic.Setup(n => n.NetworkInterfaceType).Returns(type);

            // Die Statistiken sind nochmal in einem eigenen Objekt gekapselt
            var mockStats = new Mock<IPv4InterfaceStatistics>();
            mockStats.Setup(s => s.BytesSent).Returns(sent);
            mockStats.Setup(s => s.BytesReceived).Returns(received);

            // Dem NetworkInterface beibringen, die künstlichen Stats zurückzugeben
            mockNic.Setup(n => n.GetIPv4Statistics()).Returns(mockStats.Object);

            return mockNic;
        }
    }
}










