using CommunityToolkit.Mvvm.ComponentModel;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Domain.Extensions;
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
    /// <summary>
    /// View model for the network traffic / network cards page. Polls
    /// <see cref="IWindowsNetworkInfoService"/> on a 1-second timer for active interfaces and
    /// bytes sent/received, then updates <see cref="NetworkCards"/> on the UI thread. Shows a
    /// single "not available" entry when the network is offline or an error occurs.
    /// </summary>
    public partial class ViewModelNetworkTraffic : ObservableObject, IDisposable
    {
        // =========================================================
        // 1. CONSTANTS & STATICS (Konstanten)
        // =========================================================
        #region ConstantsAndStatics

        private const string ColorDefault = "#FFFFFF";
        private const string ColorError = "#FF0000";
        private const string ImgCard = "/Resources/Visuals/Icons/LightTheme/network-interface-card_light_theme.png";
        private const string ImgDown = "/Resources/Visuals/Icons/LightTheme/download_light_theme.png";
        private const string ImgUp = "/Resources/Visuals/Icons/LightTheme/send-data-light_theme.png";

        #endregion

        // =========================================================
        // 2. FIELDS & INJECTED SERVICES (Backing-Felder und DI)
        // =========================================================
        #region FieldsAndInjectedServices

        private readonly DispatcherQueue _dispatcherQueue;
        private readonly IWindowsNetworkInfoService _networkService;
        private readonly ILocalizationService _localizationService;
        private readonly Timer _timer;

        #endregion

        // =========================================================
        // 3. PUBLIC PROPERTIES (Data & State)
        // =========================================================
        #region PublicProperties

        /// <summary>Collection of network adapters with localized names and sent/received data (human-readable size).</summary>
        public ObservableCollection<NetworkCardDisplayModel> NetworkCards { get; } = [];

        #endregion

        // =========================================================
        // 4. CONSTRUCTOR & FINALIZER (Ctor)
        // =========================================================
        #region ConstructorAndFinalizer

        /// <summary>
        /// Initializes the VM with network and localization services, captures the current
        /// dispatcher queue for UI updates, and starts a 1-second timer that polls network stats
        /// and applies results on the UI thread. Runs the first update immediately on a background thread.
        /// </summary>
        public ViewModelNetworkTraffic(
            IWindowsNetworkInfoService networkService,
            ILocalizationService localizationService)
        {
            _networkService = networkService;
            _localizationService = localizationService;
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            _timer = new Timer(1000);
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = true;
            _timer.Start();

            System.Threading.Tasks.Task.Run(() => PerformUpdate());
        }

        #endregion

        // =========================================================
        // 5. PUBLIC & PROTECTED METHODS (API)
        // =========================================================
        #region PublicAndProtectedMethods

        /// <summary>Stops and disposes the timer so the page can unload without further background updates.</summary>
        public void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }

        #endregion

        // =========================================================
        // 6. PRIVATE HELPER METHODS (Interne Hilfsmethoden)
        // =========================================================
        #region PrivateHelperMethods

        /// <summary>
        /// Updates the NetworkCards collection on the UI thread: removes adapters no longer in stats,
        /// updates or adds entries for each active interface with localized labels and formatted byte counts.
        /// If offline or stats null, switches to the single "not available" state.
        /// </summary>
        private void ApplyDataToUi(bool isNetworkAvailable, IEnumerable<NetworkStats>? stats)
        {
            if (!isNetworkAvailable || stats == null)
            {
                ShowOfflineState();
                return;
            }

            string prefixCard = _localizationService.GetString("Network_CardPrefix");
            string prefixRec = _localizationService.GetString("Network_Received");
            string prefixSent = _localizationService.GetString("Network_Sent");

            var activeIds = stats.Select(networkStat => $"{prefixCard}: {networkStat.Name}").ToList();

            var cardsToRemove = NetworkCards.Where(card => !activeIds.Contains(card.AdapterName)).ToList();

            foreach (var cardToRemove in cardsToRemove)
            {
                NetworkCards.Remove(cardToRemove);
            }

            foreach (var stat in stats)
            {
                string name = $"{prefixCard}: {stat.Name}";
                var existingCard = NetworkCards.FirstOrDefault(card => card.AdapterName == name);

                //string received = $"{prefixRec}: {FormatExtensions.ToSizeSuffix(stat.BytesReceived)}";
                //string sent = $"{prefixSent}: {FormatExtensions.ToSizeSuffix(stat.BytesSent)}";

                string received = $"{prefixRec}: {stat.BytesReceived.ToSizeSuffix()}";
                string sent = $"{prefixSent}: {stat.BytesSent.ToSizeSuffix()}";

                if (existingCard != null)
                {
                    existingCard.ReceivedData = received;
                    existingCard.SentData = sent;
                    existingCard.ForegroundColor = ColorDefault;
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

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            PerformUpdate();
        }

        /// <summary>Reads network availability and active interface stats (may throw), then marshals result to UI thread for ApplyDataToUi.</summary>
        private void PerformUpdate()
        {
            bool isAvailable = false;

            List<NetworkStats>? currentStats = null;

            try
            {
                isAvailable = _networkService.IsNetworkAvailable();

                if (isAvailable)
                    currentStats = [.. _networkService.GetActiveInterfaces()]; //_networkService.GetActiveInterfaces().ToList();
            }
            catch (Exception)
            {
                isAvailable = false;
            }

            _dispatcherQueue.TryEnqueue(() => ApplyDataToUi(isAvailable, currentStats));
        }

        /// <summary>Replaces the list with a single entry indicating network is not available, using error color, unless that state is already shown to avoid flicker.</summary>
        private void ShowOfflineState()
        {
            string prefixCard = _localizationService.GetString("Network_CardPrefix");
            string notAvail = _localizationService.GetString("Network_NotAvailable");
            string prefixRec = _localizationService.GetString("Network_Received");
            string prefixSent = _localizationService.GetString("Network_Sent");

            string fullName = $"{prefixCard}: {notAvail}";

            if (NetworkCards.Count == 1 && NetworkCards[0].AdapterName == fullName)
                return;

            NetworkCards.Clear();

            NetworkCards.Add(new NetworkCardDisplayModel
            {
                AdapterName = fullName,
                ReceivedData = $"{prefixRec}: -",
                SentData = $"{prefixSent}: -",
                ForegroundColor = ColorError,
                ImagePathNetworkCard = ImgCard,
                ImagePathReceivedData = ImgDown,
                ImagePathSendData = ImgUp
            });
        }

        #endregion
    }
}
