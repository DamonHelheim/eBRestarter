using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Domain.Extensions;
using eBRestarter.Desktop.WinUI3.Models.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace eBRestarter.Desktop.WinUI3.Views.UserControls;

public sealed partial class UC_Networktraffic : UserControl
{
    private readonly IWindowsNetworkInfoService _networkService;

    // ObservableCollection informiert die GUI automatisch bei Änderungen!
    // Das ersetzt das ständige "ItemsSource = list".
    public ObservableCollection<NetworkCardDisplayModel> NetworkCards { get; set; } = new();

    private readonly DispatcherTimer _timer;

    // Deine Theme-Variablen (sollten idealerweise aus einem ThemeService kommen)
    private string _foregroundColor = "#FFFFFF";
    private string _imgCard = "pack://application:,,,/Images/card.png";
    private string _imgRec = "pack://application:,,,/Images/down.png";
    private string _imgSend = "pack://application:,,,/Images/up.png";

    // Constructor Injection ist bei UserControls in WPF schwierig. 
    // Oft übergibt man den Service später oder nutzt einen ServiceLocator/Resolver.
    // Hier simulieren wir, dass er gesetzt wird (z.B. im MainWindow).
    public UC_Networktraffic()
    {
        InitializeComponent();

        // Dummy-Initialisierung (In echter App via DI Container auflösen!)
        // _networkService = App.ServiceProvider.GetService<INetworkInfoService>();
        // Fallback für Design-Time:
        //if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;

        // Timer statt Thread: Sicherer für UI-Updates
        //_timer = new DispatcherTimer();
        //_timer.Interval = TimeSpan.FromSeconds(1);
        //_timer.Tick += UpdateTrafficData;

        // Datenbindung vorbereiten
        icNetworkCardsList.ItemsSource = NetworkCards;
    }

    // Diese Methode muss von außen aufgerufen werden, um den Service zu injizieren
    public void Initialize(IWindowsNetworkInfoService networkService)
    {
        //_networkService = networkService;
        //_timer.Start();
    }

    private void UpdateTrafficData(object? sender, EventArgs e)
    {
        if (_networkService == null) return;

        // 1. Daten holen (Geht schnell, kann im UI Thread passieren oder async gemacht werden)
        if (!_networkService.IsNetworkAvailable())
        {
            NetworkCards.Clear();
            NetworkCards.Add(CreateOfflineModel());
            return;
        }

        var currentStats = _networkService.GetActiveInterfaces().ToList();

        // 2. Bestehende Liste aktualisieren (besser als neu erstellen -> kein Flackern)
        NetworkCards.Clear(); // Einfachste Variante. Für Pro-Level: Werte updaten statt löschen.

        foreach (var stat in currentStats)
        {
            NetworkCards.Add(new NetworkCardDisplayModel
            {
                AdapterName = $"Netzwerkkarte: {stat.Name}",
                // Hier deinen css-Helper nutzen
                ReceivedData = $"Empfangen: {FormatExtensions.ToSizeSuffix(stat.BytesReceived)}",
                SentData = $"Gesendet: {FormatExtensions.ToSizeSuffix(stat.BytesSent)}",

                ImagePathNetworkCard = _imgCard,
                ImagePathReceivedData = _imgRec,
                ImagePathSendData = _imgSend,
                ForegroundColor = _foregroundColor
            });
        }
    }

    private NetworkCardDisplayModel CreateOfflineModel()
    {
        return new NetworkCardDisplayModel
        {
            AdapterName = "Netzwerkkarte: nicht verfügbar",
            ReceivedData = "Empfangen: -",
            SentData = "Gesendet: -",
            ForegroundColor = "Red", // Warnfarbe
            ImagePathNetworkCard = _imgCard,
            ImagePathReceivedData = _imgRec,
            ImagePathSendData = _imgSend
        };
    }
}

