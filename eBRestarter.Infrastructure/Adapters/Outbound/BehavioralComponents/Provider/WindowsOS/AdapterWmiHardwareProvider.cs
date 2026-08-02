using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Management;
using System.Runtime.Versioning;
using System.Threading.Tasks;

using eBRestarter.Core.Application.BehavioralComponents.Extensions;
using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Provider.WindowsOS;

/// <summary>
/// Adapter: Driven Adapter (Outbound Provider) for querying system hardware and OS edition details via WMI.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter / Provider)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Dienstleister im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core durch Auslesen von Windows WMI-Klassen (Win32_Processor, Win32_VideoController etc.).<br/>
/// - <strong>Implementierter Port:</strong> <see cref="IOutboundPortHardwareInfoProvider"/> und <see cref="IOutboundPortOsEditionProvider"/> (aus dem Application Core).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein vorbildlicher <strong>Outbound Adapter</strong> (Taxonomie: Provider Adapter), da sie im Infrastructure-Layer liegt, Outbound Ports implementiert und vom Core angetrieben wird, um technologische WMI/Hardware-Abfragen auszuführen.
/// </para>
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class AdapterWmiHardwareProvider : IOutboundPortHardwareInfoProvider, IOutboundPortOsEditionProvider
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitive Typen & Strings ──
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
    // ── Block 1: Injizierte Abhängigkeiten (Dependencies) ──
    private readonly ILogger<AdapterWmiHardwareProvider> _logger;


    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════
    public AdapterWmiHardwareProvider(ILogger<AdapterWmiHardwareProvider> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
    }


    // ═══════════════════════════════════════════════════════
    //  8. Methods (public → private)
    // ═══════════════════════════════════════════════════════
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
                // WMI liefert KB, FormatExtensions erwartet Bytes -> * 1024
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

    public async Task<string> RetrieveOsEditionAsync()
    {
        return await Task.Run(() => RetrieveWmiValue(WmiClassOperatingSystem, WmiPropertyCaption) ?? UnknownOsEditionFallbackText).ConfigureAwait(false);
    }

    /// <summary>
    /// Helper method that utilizes WMI ManagementObjectSearcher to query properties from WMI classes.
    /// </summary>
    private string? RetrieveWmiValue(string wmiClass, string property)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher($"SELECT {property} FROM {wmiClass}");
            using var collection = searcher.Get();

            var firstObj = collection.Cast<ManagementBaseObject>().FirstOrDefault();
            return firstObj?[property]?.ToString();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, ErrorRetrievingWmiDataLogMessage, wmiClass, property);
        }

        return null;
    }
}
