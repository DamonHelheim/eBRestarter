using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.ObjectArchetypes.Models;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;
using eBRestarter.Core.Application.ObjectArchetypes.Constants;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Provider.WindowsOS;

/// <summary>
/// Adapter: Driven Adapter (Outbound Provider) for discovering installed web browsers on Windows.
/// <para>
/// <strong>Architecture Classification: OUTBOUND ADAPTER (Driven Adapter / Provider)</strong><br/>
/// - <strong>Role &amp; Responsibility:</strong> Performs OS-specific browser discovery via Registry and filesystem probes in the Infrastructure layer.<br/>
/// - <strong>Implemented Port:</strong> <see cref="IOutboundPortBrowserDiscoveryProvider"/>.<br/>
/// </para>
/// </summary>
public sealed class AdapterWindowsBrowserDiscoveryProvider : IOutboundPortBrowserDiscoveryProvider, IDisposable
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitives & strings ──
    private const string BrowserTypeNotSupportedLogMessage = "Browser type {Type} is currently not supported by the factory implementation.";
    private const string ErrorEvaluatingBrowserDetailsLogMessage = "An error occurred while evaluating details for browser type {Type}";
    private const string NotInstalledStatusText = "Not installed";

    // 30-second TTL cache prevents redundant Registry and filesystem probes triggered by UI timers.
    private static readonly TimeSpan DiscoveryCacheTimeToLive = TimeSpan.FromSeconds(30);

    // Pre-materialized array of all supported browser types to prevent per-call allocations.
    private static readonly BrowserType[] AllBrowserTypes = Enum.GetValues<BrowserType>();

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    // ── Block 1: Injected dependencies ──
    private readonly IOutboundPortBrowserFactory _browserFactory;
    private readonly ILogger<AdapterWindowsBrowserDiscoveryProvider> _logger;
    private readonly TimeProvider _timeProvider;

    // ── Block 4: Cache state ──
    private readonly SemaphoreSlim _discoveryGate = new(1, 1);
    private IReadOnlyList<BrowserInfo>? _cachedBrowsers;
    private long _cacheTimestampTicks;
    private bool _disposed;


    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Initializes a new instance of <see cref="AdapterWindowsBrowserDiscoveryProvider"/>.
    /// </summary>
    /// <param name="browserFactory">Factory for instantiating browser logic wrappers.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="timeProvider">
    /// Clock used for the discovery cache TTL. Optional so existing callers and tests keep working;
    /// the DI container injects the registered <see cref="TimeProvider.System"/> singleton.
    /// </param>
    public AdapterWindowsBrowserDiscoveryProvider(
        IOutboundPortBrowserFactory browserFactory,
        ILogger<AdapterWindowsBrowserDiscoveryProvider> logger,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(browserFactory);
        ArgumentNullException.ThrowIfNull(logger);

        _browserFactory = browserFactory;
        _logger = logger;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }


    // ═══════════════════════════════════════════════════════
    //  8. Methods (public → private)
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Releases the semaphore gate resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _discoveryGate.Dispose();
    }

    /// <summary>
    /// Asynchronously discovers installed browsers on the system, using a 30-second TTL cache.
    /// </summary>
    public async Task<IEnumerable<BrowserInfo>> FindInstalledBrowsersAsync()
    {
        if (TryReadFreshCache(out IReadOnlyList<BrowserInfo>? cached))
        {
            return cached;
        }

        // Semaphore gate prevents cache stampede when multiple UI timers tick simultaneously.
        await _discoveryGate.WaitAsync().ConfigureAwait(false);

        try
        {
            // Double-check lock pattern to verify if another thread refreshed the cache.
            if (TryReadFreshCache(out cached))
            {
                return cached;
            }

            IReadOnlyList<BrowserInfo> discovered = DiscoverInstalledBrowsers();

            _cachedBrowsers = discovered;
            Volatile.Write(ref _cacheTimestampTicks, _timeProvider.GetUtcNow().UtcTicks);

            return discovered;
        }
        finally
        {
            _discoveryGate.Release();
        }
    }

    /// <summary>
    /// Executes the actual registry and file system probing for every supported browser type.
    /// </summary>
    private List<BrowserInfo> DiscoverInstalledBrowsers()
    {
        // Pre-allocate list capacity matching exact browser type count.
        var browsers = new List<BrowserInfo>(AllBrowserTypes.Length);

        foreach (BrowserType type in AllBrowserTypes)
        {
            try
            {
                IOutboundPortBrowser browserLogic = _browserFactory.Create(type);

                bool isInstalled = browserLogic.IsInstalled;
                string version = isInstalled ? browserLogic.BrowserVersion : NotInstalledStatusText;
                string displayName = browserLogic.DisplayName;
                string iconPath = browserLogic.IconPath;
                string downloadUrl = browserLogic.DownloadUrl;

                var browserInfo = new BrowserInfo
                {
                    Type = type,
                    Name = displayName,
                    IsInstalled = isInstalled,
                    Version = version,
                    IconPath = iconPath,
                    DownloadUrl = downloadUrl
                };

                browsers.Add(browserInfo);
            }
            catch (NotSupportedException exception)
            {
                _logger.LogWarning(LogEventIds.Browser.BrowserTypeNotSupported, exception, BrowserTypeNotSupportedLogMessage, type);
            }
            catch (Exception exception)
            {
                _logger.LogError(LogEventIds.Browser.BrowserDetailEvaluationFailed, exception, ErrorEvaluatingBrowserDetailsLogMessage, type);
            }
        }

        return browsers;
    }

    /// <summary>
    /// Returns the cached snapshot when it is still within <see cref="DiscoveryCacheTimeToLive"/>.
    /// </summary>
    private bool TryReadFreshCache(out IReadOnlyList<BrowserInfo> cached)
    {
        IReadOnlyList<BrowserInfo>? snapshot = _cachedBrowsers;

        if (snapshot is not null
            && _timeProvider.GetUtcNow().UtcTicks - Volatile.Read(ref _cacheTimestampTicks) < DiscoveryCacheTimeToLive.Ticks)
        {
            cached = snapshot;
            return true;
        }

        cached = [];
        return false;
    }
}
