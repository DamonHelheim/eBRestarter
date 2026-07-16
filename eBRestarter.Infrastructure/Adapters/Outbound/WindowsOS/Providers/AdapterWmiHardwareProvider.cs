using eBRestarter.Core.Application.BehavioralComponents.Extensions;
using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using Microsoft.Extensions.Logging;
using System.Management;

namespace eBRestarter.Infrastructure.Adapters.Outbound.WindowsOS.Providers;

/// <summary>
/// Adapter: Driven Adapter (Outbound Provider) for querying system hardware and OS edition details via WMI.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter / Provider)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Dienstleister im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core durch Auslesen von Windows WMI-Klassen (Win32_Processor, Win32_VideoController etc.).<br/>
/// - <strong>Implementierte Ports:</strong> <see cref="IOutboundPortHardwareInfoProvider"/> und <see cref="IOutboundPortOsEditionProvider"/> (aus dem Application Core).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein vorbildlicher <strong>Outbound Adapter</strong> (Taxonomie: Provider Adapter), da sie im Infrastructure-Layer liegt, Outbound Ports implementiert und vom Core angetrieben wird, um technologische WMI/Hardware-Abfragen auszuführen.
/// </para>
/// </summary>
public sealed class AdapterWmiHardwareProvider(ILogger<AdapterWmiHardwareProvider> logger) : IOutboundPortHardwareInfoProvider, IOutboundPortOsEditionProvider
{
    private readonly ILogger<AdapterWmiHardwareProvider> _logger = logger;

    public async Task<HardwareInfo> RetrieveHardwareInfoAsync()
    {
        return await Task.Run(() =>
        {
            var cpu = RetrieveWmiValue("Win32_Processor", "Name") ?? "N/A";
            var gpu = RetrieveWmiValue("Win32_VideoController", "Name") ?? "N/A";
            var ramRaw = RetrieveWmiValue("Win32_OperatingSystem", "TotalVisibleMemorySize");

            string ramFormatted = "N/A";

            if (long.TryParse(ramRaw, out long ramKb))
            {
                // WMI liefert KB, FormatExtensions erwartet Bytes -> * 1024
                ramFormatted = (ramKb * 1024).ToSizeSuffix();
            }

            return new HardwareInfo
            {
                ProcessorName = cpu,
                GraphicsCardName = gpu,
                InstalledRam = ramFormatted
            };
        });
    }

    public async Task<string> RetrieveOsEditionAsync()
    {
        return await Task.Run(() => RetrieveWmiValue("Win32_OperatingSystem", "Caption") ?? "Windows (Unkwone)");
    }

    // Private Hilfsmethode, um Code-Duplizierung zu vermeiden (DRY Principle)
    private string? RetrieveWmiValue(string wmiClass, string property)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher($"SELECT {property} FROM {wmiClass}");
            using var collection = searcher.Get();
            using var enumerator = collection.GetEnumerator();

            if (enumerator.MoveNext())
            {
                return enumerator.Current[property]?.ToString();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving WMI data: {Class}.{Property}", wmiClass, property);
        }
        return null;
    }
}





