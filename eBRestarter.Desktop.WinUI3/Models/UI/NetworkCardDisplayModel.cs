using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Desktop.WinUI3.Models.UI
{
    // Record: Immutable by default, perfekt für Listen in der UI.
    public partial class NetworkCardDisplayModel : ObservableObject
    {
        // Ein eindeutiger Identifier (wichtig für das Updaten der Liste)
        public string Id => AdapterName;

        [ObservableProperty] public partial string AdapterName { get; set; } = string.Empty;
        [ObservableProperty] public partial string ReceivedData { get; set; } = string.Empty;
        [ObservableProperty] public partial string SentData { get; set; } = string.Empty;

        // UI-Pfade (WinUI 3 nutzt ms-appx:/// statt pack://)
        [ObservableProperty] public partial string ImagePathNetworkCard { get; set; } = string.Empty;
        [ObservableProperty] public partial string ImagePathReceivedData { get; set; } = string.Empty;
        [ObservableProperty] public partial string ImagePathSendData { get; set; } = string.Empty;

        [ObservableProperty] public partial string ForegroundColor { get; set; } = "#000000";
    }
}
