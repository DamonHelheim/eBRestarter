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

        public ObservableCollection<NetworkCardDisplayModel> NetworkCards { get; } = [];

        #endregion

        // =========================================================
        // 4. CONSTRUCTOR & FINALIZER (Ctor)
        // =========================================================
        #region ConstructorAndFinalizer

        public ViewModelNetworkTraffic(
            IWindowsNetworkInfoService networkService,
            ILocalizationService localizationService)
        {
            _networkService = networkService;
            _localizationService = localizationService;
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            _timer = new System.Timers.Timer(1000);
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

            var activeIds = stats.Select(x => $"{prefixCard}: {x.Name}").ToList();
            var itemsToRemove = NetworkCards.Where(x => !activeIds.Contains(x.AdapterName)).ToList();
            foreach (var item in itemsToRemove)
                NetworkCards.Remove(item);

            foreach (var stat in stats)
            {
                string name = $"{prefixCard}: {stat.Name}";
                var existingItem = NetworkCards.FirstOrDefault(x => x.AdapterName == name);
                string received = $"{prefixRec}: {FormatExtensions.ToSizeSuffix(stat.BytesReceived)}";
                string sent = $"{prefixSent}: {FormatExtensions.ToSizeSuffix(stat.BytesSent)}";

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

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            PerformUpdate();
        }

        private void PerformUpdate()
        {
            bool isAvailable = false;
            List<NetworkStats>? currentStats = null;
            try
            {
                isAvailable = _networkService.IsNetworkAvailable();
                if (isAvailable)
                    currentStats = _networkService.GetActiveInterfaces().ToList();
            }
            catch (Exception)
            {
                isAvailable = false;
            }
            _dispatcherQueue.TryEnqueue(() => ApplyDataToUi(isAvailable, currentStats));
        }

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
