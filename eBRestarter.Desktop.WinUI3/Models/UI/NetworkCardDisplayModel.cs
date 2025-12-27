using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Desktop.WinUI3.Models.UI
{
    // Record: Immutable by default, perfekt für Listen in der UI.
    public record NetworkCardDisplayModel
    {
        public string AdapterName { get; set; } = string.Empty;
        public string ReceivedData { get; set; } = string.Empty;
        public string SentData { get; set; } = string.Empty;

        // UI-spezifische Eigenschaften
        public string ImagePathNetworkCard { get; set; } = string.Empty;
        public string ImagePathReceivedData { get; set; } = string.Empty;
        public string ImagePathSendData { get; set; } = string.Empty;
        public string ForegroundColor { get; set; } = "#000000";
    }
}
