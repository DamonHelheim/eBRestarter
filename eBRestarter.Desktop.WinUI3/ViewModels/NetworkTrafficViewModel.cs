using CommunityToolkit.Mvvm.ComponentModel;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Domain.Extensions; // Falls du das FormatExtensions Model brauchst
using eBRestarter.Core.Domain.Models.Records;
using eBRestarter.Desktop.WinUI3.Models.UI;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    public partial class NetworkTrafficViewModel : ObservableObject, IDisposable
    {
        private readonly IWindowsNetworkInfoService _networkService;
        private readonly System.Timers.Timer _timer;
        private readonly DispatcherQueue _dispatcherQueue;

        public ObservableCollection<NetworkCardDisplayModel> NetworkCards { get; } = [];

        // Pfade als Konstanten
        private const string ImgCard = "/Resources/Visuals/Icons/LightTheme/network-interface-card_light_theme.png";
        private const string ImgDown = "/Resources/Visuals/Icons/LightTheme/download_light_theme.png";
        private const string ImgUp = "/Resources/Visuals/Icons/LightTheme/send-data-light_theme.png";
        private const string ColorDefault = "#FFFFFF";
        private const string ColorError = "#FF0000";

        public NetworkTrafficViewModel(IWindowsNetworkInfoService networkService)
        {
            _networkService = networkService;
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            // Timer Setup
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = true;
            _timer.Start();

            // Initiales Update (Fire & Forget im Hintergrund starten)
            System.Threading.Tasks.Task.Run(() => PerformUpdate());
        }

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            PerformUpdate();
        }

        private void PerformUpdate()
        {
            // ---------------------------------------------------------
            // SCHRITT 1: DATEN HOLEN (Auf dem Hintergrund-Thread!)
            // ---------------------------------------------------------

            bool isAvailable = false;
            // KORREKTUR: Hier den echten Typen 'NetworkStats' statt 'dynamic' verwenden
            List<NetworkStats>? currentStats = null;

            try
            {
                isAvailable = _networkService.IsNetworkAvailable();
                if (isAvailable)
                {
                    // Jetzt passt der Typ: GetActiveInterfaces gibt NetworkStats zurück, und wir speichern es in einer Liste von NetworkStats
                    currentStats = _networkService.GetActiveInterfaces().ToList();
                }
            }
            catch (Exception)
            {
                isAvailable = false;
            }

            // ---------------------------------------------------------
            // SCHRITT 2: UI AKTUALISIEREN (Auf dem UI-Thread)
            // ---------------------------------------------------------
            _dispatcherQueue.TryEnqueue(() =>
            {
                ApplyDataToUi(isAvailable, currentStats);
            });
        }

        // KORREKTUR: Auch hier im Parameter den echten Typen nutzen
        private void ApplyDataToUi(bool isNetworkAvailable, IEnumerable<NetworkStats>? stats)
        {
            if (!isNetworkAvailable || stats == null)
            {
                ShowOfflineState();
                return;
            }

            // SMART UPDATE LOGIK

            // 1. Karten identifizieren, die nicht mehr da sind -> Löschen
            var activeIds = stats.Select(x => $"Netzwerkkarte: {x.Name}").ToList();
            var itemsToRemove = NetworkCards.Where(x => !activeIds.Contains(x.AdapterName)).ToList();

            foreach (var item in itemsToRemove)
            {
                NetworkCards.Remove(item);
            }

            // 2. Vorhandene aktualisieren oder neue hinzufügen
            foreach (var stat in stats)
            {
                string name = $"Netzwerkkarte: {stat.Name}";
                var existingItem = NetworkCards.FirstOrDefault(x => x.AdapterName == name);

                // Da 'stat' jetzt typisiert ist, hast du hier auch IntelliSense!
                // Ggf. musst du (long) entfernen, falls BytesReceived schon ein long ist.
                string received = $"Empfangen: {FormatExtensions.ToSizeSuffix(stat.BytesReceived)}";
                string sent = $"Gesendet: {FormatExtensions.ToSizeSuffix(stat.BytesSent)}";

                if (existingItem != null)
                {
                    existingItem.ReceivedData = received;
                    existingItem.SentData = sent;
                    existingItem.ForegroundColor = ColorDefault;
                }
                else
                {
                    NetworkCards.Add(new NetworkCardDisplayModel
                    {
                        AdapterName = name,
                        ReceivedData = received,
                        SentData = sent,
                        ImagePathNetworkCard = ImgCard,
                        ImagePathReceivedData = ImgDown,
                        ImagePathSendData = ImgUp,
                        ForegroundColor = ColorDefault
                    });
                }
            }
        }

        private void ShowOfflineState()
        {
            // Prüfen ob wir schon im Offline State sind, um unnötiges Clear/Add zu vermeiden
            if (NetworkCards.Count == 1 && NetworkCards[0].AdapterName == "Netzwerkkarte: nicht verfügbar")
                return;

            NetworkCards.Clear();
            NetworkCards.Add(new NetworkCardDisplayModel
            {
                AdapterName = "Netzwerkkarte: nicht verfügbar",
                ReceivedData = "Empfangen: -",
                SentData = "Gesendet: -",
                ForegroundColor = ColorError,
                ImagePathNetworkCard = ImgCard,
                ImagePathReceivedData = ImgDown,
                ImagePathSendData = ImgUp
            });
        }

        public void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }
    }
}