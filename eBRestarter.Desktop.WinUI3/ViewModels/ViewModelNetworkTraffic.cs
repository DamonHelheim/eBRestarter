using CommunityToolkit.Mvvm.ComponentModel;
using eBRestarter.Core.Application.Extensions;
using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Core.Application.Ports.Inbound.Providers;
using eBRestarter.Desktop.WinUI3.Models.UI;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace eBRestarter.Desktop.WinUI3.ViewModels;

/// <summary>
/// View model for the network traffic / network cards page. Polls
/// <see cref="IInboundPortNetworkInfoProvider"/> on a timer for active interfaces and
/// bytes sent/received, then updates <see cref="NetworkCards"/> on the UI thread. Shows a
/// single "not available" entry when the network is offline or an error occurs.
/// </summary>
public sealed partial class ViewModelNetworkTraffic : ObservableObject, IDisposable
{
    private const string NetworkCardDefaultForegroundHex = "#FFFFFF";

    private const string NetworkCardIconPath = "/Resources/Visuals/Icons/LightTheme/network-interface-card_light_theme.png";

    private const string NetworkOfflineForegroundHex = "#FF0000";

    private const double NetworkStatsPollIntervalMilliseconds = 1000;

    private const string ReceivedDataIconPath = "/Resources/Visuals/Icons/LightTheme/download_light_theme.png";

    private const string SentDataIconPath = "/Resources/Visuals/Icons/LightTheme/send-data-light_theme.png";

    private readonly IInboundPortLocalizationProvider _localizationService;

    private readonly IInboundPortNetworkInfoProvider _networkService;

    private readonly DispatcherQueue _dispatcherQueue;

    private readonly Timer _timer;

    /// <summary>Collection of network adapters with localized names and sent/received data (human-readable size).</summary>
    public ObservableCollection<NetworkCardDisplayModel> NetworkCards { get; } = [];


    /// <summary>
    /// Initializes the VM with network and localization services, captures the current
    /// dispatcher queue for UI updates, and starts a timer that polls network stats
    /// and applies results on the UI thread. Runs the first update immediately on a background thread.
    /// </summary>
    public ViewModelNetworkTraffic(
        IInboundPortNetworkInfoProvider networkService,
        IInboundPortLocalizationProvider LocalizationProvider)
    {
        ArgumentNullException.ThrowIfNull(networkService);
        ArgumentNullException.ThrowIfNull(LocalizationProvider);

        _networkService = networkService;
        _localizationService = LocalizationProvider;

        _dispatcherQueue =
            DispatcherQueue.GetForCurrentThread()
            ?? throw new InvalidOperationException(
                $"{nameof(ViewModelNetworkTraffic)} must be constructed on a thread with a WinUI DispatcherQueue (UI thread).");

        _timer = new Timer(NetworkStatsPollIntervalMilliseconds);
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = true;
        _timer.Start();

        Task.Run(PerformUpdate);
    }

    /// <summary>Stops and disposes the timer so the page can unload without further background updates.</summary>
    public void Dispose()
    {
        _timer.Stop();
        _timer.Elapsed -= OnTimerElapsed;
        _timer.Dispose();

        GC.SuppressFinalize(this);
    }

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

        string adapterNamePrefix = _localizationService.RetrieveString("Network_CardPrefix");
        string receivedPrefix = _localizationService.RetrieveString("Network_Received");
        string sentPrefix = _localizationService.RetrieveString("Network_Sent");

        var activeIds = stats.Select(networkStat => $"{adapterNamePrefix}: {networkStat.Name}").ToList();

        var cardsToRemove = NetworkCards.Where(card => !activeIds.Contains(card.AdapterName)).ToList();

        foreach (var cardToRemove in cardsToRemove)
            NetworkCards.Remove(cardToRemove);

        foreach (var stat in stats)
        {
            string name = $"{adapterNamePrefix}: {stat.Name}";
            var existingCard = NetworkCards.FirstOrDefault(card => card.AdapterName == name);

            string received = $"{receivedPrefix}: {stat.BytesReceived.ToSizeSuffix()}";
            string sent = $"{sentPrefix}: {stat.BytesSent.ToSizeSuffix()}";

            if (existingCard != null)
            {
                existingCard.ReceivedData = received;
                existingCard.SentData = sent;
                existingCard.ForegroundColor = NetworkCardDefaultForegroundHex;
            }
            else
            {
                NetworkCards.Add(new NetworkCardDisplayModel
                {
                    AdapterName = name,
                    ReceivedData = received,
                    SentData = sent,
                    ImagePathNetworkCard = NetworkCardIconPath,
                    ImagePathReceivedData = ReceivedDataIconPath,
                    ImagePathSendData = SentDataIconPath,
                    ForegroundColor = NetworkCardDefaultForegroundHex
                });
            }
        }
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs elapsedEventArgs)
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
                currentStats = [.. _networkService.RetrieveActiveInterfaces()];
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            isAvailable = false;
        }

        _dispatcherQueue.TryEnqueue(() => ApplyDataToUi(isAvailable, currentStats));
    }

    /// <summary>Replaces the list with a single entry indicating network is not available, using error color, unless that state is already shown to avoid flicker.</summary>
    private void ShowOfflineState()
    {
        string adapterNamePrefix = _localizationService.RetrieveString("Network_CardPrefix");
        string notAvailableLabel = _localizationService.RetrieveString("Network_NotAvailable");
        string receivedPrefix = _localizationService.RetrieveString("Network_Received");
        string sentPrefix = _localizationService.RetrieveString("Network_Sent");

        string fullName = $"{adapterNamePrefix}: {notAvailableLabel}";

        if (NetworkCards.Count == 1 && NetworkCards[0].AdapterName == fullName)
            return;

        NetworkCards.Clear();

        NetworkCards.Add(new NetworkCardDisplayModel
        {
            AdapterName = fullName,
            ReceivedData = $"{receivedPrefix}: -",
            SentData = $"{sentPrefix}: -",
            ForegroundColor = NetworkOfflineForegroundHex,
            ImagePathNetworkCard = NetworkCardIconPath,
            ImagePathReceivedData = ReceivedDataIconPath,
            ImagePathSendData = SentDataIconPath
        });
    }
}









