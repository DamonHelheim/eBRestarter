using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Domain.Models.Records.Config
{
    /// <summary>
    /// Repräsentiert die Struktur der config.json für die Chrome-Erweiterung
    /// </summary>
    public record ExtensionConfigDto
    {
        public string LANGUAGE { get; init; } = "DE";
        public string ZIEL_URL { get; init; } = "https://www.google.de";
        public int WARTEZEIT_MS { get; init; } = 180000;
    }
}
