using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Domain.Models.Records
{
    // Record für Hardware-Daten
    public record HardwareInfo
    {
        public string ProcessorName { get; init; } = "Unbekannt";
        public string GraphicsCardName { get; init; } = "Unbekannt";
        public string InstalledRam { get; init; } = "0 bytes";
    }
}
