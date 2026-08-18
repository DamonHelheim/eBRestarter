using Microsoft.Extensions.Logging;
using System;
using System.Management;
using System.Runtime.Versioning;
using System.Threading.Tasks;

using eBRestarter.Core.Application.BehavioralComponents.Extensions;
using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.ObjectArchetypes.Constants;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Provider.WindowsOS;

/// <summary>
/// Adapter: Driven Adapter (Outbound Provider) for querying system hardware and OS edition details via WMI.
/// <para>
/// <strong>Architecture Classification: OUTBOUND ADAPTER (Driven Adapter / Provider)</strong><br/>
/// - <strong>Role &amp; Responsibility:</strong> Queries Windows WMI classes (Win32_Processor, Win32_VideoController, Win32_OperatingSystem) in the Infrastructure layer.<br/>
/// - <strong>Implemented Ports:</strong> <see cref="IOutboundPortHardwareInfoProvider"/> and <see cref="IOutboundPortOsEditionProvider"/>.<br/>
/// </para>
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class AdapterWmiHardwareProvider : IOutboundPortHardwareInfoProvider, IOutboundPortOsEditionProvider
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitives & strings ──
    private const long BytesPerKilobyte = 1024;
    private const string ErrorRetrievingWmiDataLogMessage = "Error retrieving WMI data: {Class}.{Property}";
    private const string NotAvailableFallbackText = "N/A";
    private const string UnknownOsEditionFallbackText = "Windows (Unknown)";
    private const string WmiClassOperatingSystem = "Win32_OperatingSystem";
    private const string WmiClassProcessor = "Win32_Processor";
    private const string WmiClassVideoController = "Win32_VideoController";
    private const string WmiPropertyCaption = "Caption";
    private const string WmiPropertyName = "Name";
    private const string WmiPropertyTotalVisibleMemorySize = "TotalVisibleMemorySize";

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    // ── Block 1: Injected dependencies ──
    private readonly ILogger<AdapterWmiHardwareProvider> _logger;


    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Initializes a new instance of <see cref="AdapterWmiHardwareProvider"/>.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public AdapterWmiHardwareProvider(ILogger<AdapterWmiHardwareProvider> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
    }


    // ═══════════════════════════════════════════════════════
    //  8. Methods (public → private)
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Asynchronously queries WMI for CPU, GPU, and installed RAM details.
    /// </summary>
    public async Task<HardwareInfo> RetrieveHardwareInfoAsync()
    {
        return await Task.Run(() =>
        {
            var cpu = RetrieveWmiValue(WmiClassProcessor, WmiPropertyName) ?? NotAvailableFallbackText;
            var gpu = RetrieveWmiValue(WmiClassVideoController, WmiPropertyName) ?? NotAvailableFallbackText;
            var ramRaw = RetrieveWmiValue(WmiClassOperatingSystem, WmiPropertyTotalVisibleMemorySize);

            string ramFormatted = NotAvailableFallbackText;

            if (long.TryParse(ramRaw, out long ramKb))
            {
                // WMI reports KB; convert to bytes (* 1024) for size formatting.
                ramFormatted = (ramKb * BytesPerKilobyte).ToSizeSuffix();
            }

            return new HardwareInfo
            {
                ProcessorName = cpu,
                GraphicsCardName = gpu,
                InstalledRam = ramFormatted
            };
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously queries WMI for the Windows OS caption string.
    /// </summary>
    public async Task<string> RetrieveOsEditionAsync()
    {
        return await Task.Run(() => RetrieveWmiValue(WmiClassOperatingSystem, WmiPropertyCaption) ?? UnknownOsEditionFallbackText).ConfigureAwait(false);
    }

    /// <summary>
    /// Helper method that utilizes WMI ManagementObjectSearcher to query properties from WMI classes.
    /// </summary>
    /// <param name="wmiClass">WMI class name (e.g. Win32_Processor).</param>
    /// <param name="property">Property name to query.</param>
    private string? RetrieveWmiValue(string wmiClass, string property)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher($"SELECT {property} FROM {wmiClass}");
            using var collection = searcher.Get();

            // COM wrapper cleanup: Enumerator and ManagementBaseObject implement IDisposable and require explicit disposal.
            using var enumerator = collection.GetEnumerator();

            if (!enumerator.MoveNext())
            {
                return null;
            }

            using var managementObject = enumerator.Current;

            return managementObject[property]?.ToString();
        }
        catch (Exception exception)
        {
            _logger.LogError(LogEventIds.OperatingSystem.WmiQueryFailed, exception, ErrorRetrievingWmiDataLogMessage, wmiClass, property);
        }

        return null;
    }
}
