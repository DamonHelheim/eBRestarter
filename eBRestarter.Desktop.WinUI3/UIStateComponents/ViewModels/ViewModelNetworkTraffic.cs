using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

using eBRestarter.Core.Application.BehavioralComponents.Extensions;
using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Desktop.WinUI3.ObjectArchetypes.ObservableModel;
using Microsoft.Extensions.Logging;
using eBRestarter.Core.Application.ObjectArchetypes.Constants;

namespace eBRestarter.Desktop.WinUI3.ViewModels;

/// <summary>
/// View model for the network traffic / network cards page. Polls
/// <see cref="IInboundPortNetworkInfoProvider"/> on a timer for active interfaces and
/// bytes sent/received, then updates <see cref="NetworkCards"/> on the UI thread. Shows a
/// single "not available" entry when the network is offline or an error occurs.
/// </summary>
public sealed partial class ViewModelNetworkTraffic : ObservableObject, IDisposable
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    private const string NetworkCardDefaultForegroundHex = "#FFFFFF";
    private const string NetworkCardIconPath = "/Resources/Visuals/Icons/LightTheme/network-interface-card_light_theme.png";
    private const string NetworkCardPrefixResourceKey = "Network_CardPrefix";
    private const string NetworkNotAvailableResourceKey = "Network_NotAvailable";
    private const string NetworkOfflineForegroundHex = "#FF0000";
    private const string NetworkReceivedResourceKey = "Network_Received";
    private const string NetworkSentResourceKey = "Network_Sent";
    private const double NetworkStatsPollIntervalMilliseconds = 1000;
    private const string OfflineDataPlaceholder = "-";
    private const string ReceivedDataIconPath = "/Resources/Visuals/Icons/LightTheme/download_light_theme.png";
    private const string SentDataIconPath = "/Resources/Visuals/Icons/LightTheme/send-data-light_theme.png";

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    private readonly IInboundPortLocalizationProvider _localizationService;
    private readonly ILogger<ViewModelNetworkTraffic> _logger;
    private readonly IInboundPortNetworkInfoProvider _networkService;

    private readonly DispatcherQueue _dispatcherQueue;
    private readonly Timer _timer;

    private volatile bool _disposed;


    // ═══════════════════════════════════════════════════════
    //  4. Properties
    // ═══════════════════════════════════════════════════════
    /// <summary>Collection of network adapters with localized names and sent/received data (human-readable size).</summary>
    public ObservableCollection<NetworkCardDisplayModel> NetworkCards { get; } = [];

    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Initializes the VM with network and localization services, captures the current
    /// dispatcher queue for UI updates, and starts a timer that polls network stats
    /// and applies results on the UI thread. Runs the first update immediately on a background thread.
    /// </summary>
    public ViewModelNetworkTraffic(
        IInboundPortLocalizationProvider localizationService,
        ILogger<ViewModelNetworkTraffic> logger,
        IInboundPortNetworkInfoProvider networkService)
    {
        ArgumentNullException.ThrowIfNull(localizationService);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(networkService);

        _localizationService = localizationService;
        _logger = logger;
        _networkService = networkService;

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

    // ═══════════════════════════════════════════════════════
    //  8. Methods (public → private)
    // ═══════════════════════════════════════════════════════
    /// <summary>Stops and disposes the timer so the view can be torn down without further background updates.</summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _timer.Stop();
        _timer.Elapsed -= OnTimerElapsed;
        _timer.Dispose();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Resumes polling after the hosting view became visible again.
    /// </summary>
    /// <remarks>
    /// The hosting page uses <c>NavigationCacheMode="Required"</c>, so the view survives navigation.
    /// Pausing instead of disposing on unload keeps the 1-second poll off the CPU while the page is
    /// invisible, without leaving a dead view model behind when the user navigates back.
    /// </remarks>
    public void StartPolling()
    {
        if (_disposed)
        {
            return;
        }

        _timer.Start();
        Task.Run(PerformUpdate);
    }

    /// <summary>Pauses polling while the hosting view is not visible (Guide Kap. 22.6).</summary>
    public void StopPolling()
    {
        if (_disposed)
        {
            return;
        }

        _timer.Stop();
    }

    /// <summary>
    /// Updates the NetworkCards collection on the UI thread: removes adapters no longer in stats,
    /// updates or adds entries for each active interface with localized labels and formatted byte counts.
    /// If offline or stats null, switches to the single "not available" state.
    /// </summary>
    private void ApplyDataToUi(bool isNetworkAvailable, IEnumerable<NetworkStats>? stats)
    {
        if (!isNetworkAvailable || stats is null)
        {
            ShowOfflineState();
            return;
        }

        string adapterNamePrefix = _localizationService.RetrieveString(NetworkCardPrefixResourceKey);
        string receivedPrefix = _localizationService.RetrieveString(NetworkReceivedResourceKey);
        string sentPrefix = _localizationService.RetrieveString(NetworkSentResourceKey);

        // ✅ .NET 10: Use HashSet for O(1) adapter lookup instead of O(N) List.Contains
        var activeIds = stats.Select(networkStat => $"{adapterNamePrefix}: {networkStat.Name}").ToHashSet();

        var cardsToRemove = NetworkCards.Where(card => !activeIds.Contains(card.AdapterName)).ToList();

        foreach (var cardToRemove in cardsToRemove)
        {
            NetworkCards.Remove(cardToRemove);
        }

        foreach (var stat in stats)
        {
            string name = $"{adapterNamePrefix}: {stat.Name}";
            var existingCard = NetworkCards.FirstOrDefault(card => card.AdapterName == name);

            string received = $"{receivedPrefix}: {stat.BytesReceived.ToSizeSuffix()}";
            string sent = $"{sentPrefix}: {stat.BytesSent.ToSizeSuffix()}";

            if (existingCard is not null)
            {
                existingCard.ReceivedData = received;
                existingCard.SentData = sent;
                existingCard.ForegroundColor = NetworkCardDefaultForegroundHex;
                continue;
            }

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

    /// <summary>
    /// Triggers network statistics update on timer tick.
    /// </summary>
    /// <param name="sender">The timer instance.</param>
    /// <param name="elapsedEventArgs">Event arguments associated with the timer tick.</param>
    private void OnTimerElapsed(object? sender, ElapsedEventArgs elapsedEventArgs)
    {
        // Guard against timer callbacks firing after disposal.
        if (_disposed)
        {
            return;
        }

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
            {
                currentStats = [.. _networkService.RetrieveActiveInterfaces()];
            }
        }
        catch (Exception exception)
        {
            // Logging guideline: Debug log level is used because this path can be hit every second; Warning would flood the log.
            _logger.LogDebug(
                LogEventIds.UserInterface.ViewModelOperationFailed,
                "Reading network interface statistics failed: {Reason}",
                exception.Message);

            isAvailable = false;
        }

        _dispatcherQueue.TryEnqueue(() => ApplyDataToUi(isAvailable, currentStats));
    }

    /// <summary>Replaces the list with a single entry indicating network is not available, using error color, unless that state is already shown to avoid flicker.</summary>
    private void ShowOfflineState()
    {
        string adapterNamePrefix = _localizationService.RetrieveString(NetworkCardPrefixResourceKey);
        string notAvailableLabel = _localizationService.RetrieveString(NetworkNotAvailableResourceKey);
        string receivedPrefix = _localizationService.RetrieveString(NetworkReceivedResourceKey);
        string sentPrefix = _localizationService.RetrieveString(NetworkSentResourceKey);

        string fullName = $"{adapterNamePrefix}: {notAvailableLabel}";

        if (NetworkCards.Count == 1 && NetworkCards[0].AdapterName == fullName)
        {
            return;
        }

        NetworkCards.Clear();

        NetworkCards.Add(new NetworkCardDisplayModel
        {
            AdapterName = fullName,
            ReceivedData = $"{receivedPrefix}: {OfflineDataPlaceholder}",
            SentData = $"{sentPrefix}: {OfflineDataPlaceholder}",
            ForegroundColor = NetworkOfflineForegroundHex,
            ImagePathNetworkCard = NetworkCardIconPath,
            ImagePathReceivedData = ReceivedDataIconPath,
            ImagePathSendData = SentDataIconPath
        });
    }
}
