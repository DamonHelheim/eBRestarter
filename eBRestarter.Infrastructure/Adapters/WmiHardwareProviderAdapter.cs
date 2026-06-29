using eBRestarter.Core.Application.Extensions;
using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Core.Application.Ports.Outbound.Application;
using Microsoft.Extensions.Logging;
using System.Management;

namespace eBRestarter.Infrastructure.Adapters
{
    // Dieser Service kümmert sich NUR um WMI (Win32_Processor, Win32_VideoController, etc.)
    public sealed class WmiHardwareProviderAdapter(ILogger<WmiHardwareProviderAdapter> logger) : IHardwareInfoPort, IOsEditionPort
    {
        private readonly ILogger<WmiHardwareProviderAdapter> _logger = logger;

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
}





