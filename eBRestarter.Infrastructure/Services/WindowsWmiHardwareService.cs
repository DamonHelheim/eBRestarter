using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Extensions;
using eBRestarter.Core.Application.Models.Records;
using Microsoft.Extensions.Logging;
using System.Management;

namespace eBRestarter.Infrastructure.Services
{
    // Dieser Service kümmert sich NUR um WMI (Win32_Processor, Win32_VideoController, etc.)
    public class WindowsWmiHardwareService(ILogger<WindowsWmiHardwareService> logger) : IHardwareInfoService, IOsEditionService
    {
        private readonly ILogger<WindowsWmiHardwareService> _logger = logger;

        public async Task<HardwareInfo> GetHardwareInfoAsync()
        {
            return await Task.Run(() =>
            {
                var cpu = GetWmiValue("Win32_Processor", "Name") ?? "N/A";
                var gpu = GetWmiValue("Win32_VideoController", "Name") ?? "N/A";
                var ramRaw = GetWmiValue("Win32_OperatingSystem", "TotalVisibleMemorySize");

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

        public async Task<string> GetOsEditionAsync()
        {
            return await Task.Run(() => GetWmiValue("Win32_OperatingSystem", "Caption") ?? "Windows (Unbekannt)");
        }

        // Private Hilfsmethode, um Code-Duplizierung zu vermeiden (DRY Principle)
        private string? GetWmiValue(string wmiClass, string property)
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
                _logger.LogError(ex, "Fehler beim Abrufen von WMI Daten: {Class}.{Property}", wmiClass, property);
            }
            return null;
        }
    }
}
