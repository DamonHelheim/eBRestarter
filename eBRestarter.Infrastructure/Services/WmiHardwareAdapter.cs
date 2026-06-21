using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Extensions;
using eBRestarter.Core.Application.Models.Records;
using Microsoft.Extensions.Logging;
using System.Management;

namespace eBRestarter.Infrastructure.Services
{
    // Dieser Service kümmert sich NUR um WMI (Win32_Processor, Win32_VideoController, etc.)
    public class WmiHardwareAdapter(ILogger<WmiHardwareAdapter> logger) : IHardwareInfoService, IOsEditionService
    {
        private readonly ILogger<WmiHardwareAdapter> _logger = logger;

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

                // SonarQube Fix: Statt einer foreach-Schleife, die nach dem 1. Durchlauf abbricht,
                // fragen wir einfach gezielt nur das erste Element ab.
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
