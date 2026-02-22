using CommunityToolkit.Mvvm.ComponentModel;

namespace eBRestarter.Desktop.WinUI3.Models.UI;

/// <summary>
/// Display model for a network card entry in the UI (immutable-style record for list binding).
/// </summary>
public partial class NetworkCardDisplayModel : ObservableObject
{
    // =========================================================
    // 1. OBSERVABLE PROPERTIES (MVVM State)
    // =========================================================
    #region ObservableProperties

    [ObservableProperty] public partial string AdapterName { get; set; } = string.Empty;
    [ObservableProperty] public partial string ReceivedData { get; set; } = string.Empty;
    [ObservableProperty] public partial string SentData { get; set; } = string.Empty;
    [ObservableProperty] public partial string ImagePathNetworkCard { get; set; } = string.Empty;
    [ObservableProperty] public partial string ImagePathReceivedData { get; set; } = string.Empty;
    [ObservableProperty] public partial string ImagePathSendData { get; set; } = string.Empty;
    [ObservableProperty] public partial string ForegroundColor { get; set; } = "#000000";

    #endregion

    // =========================================================
    // 2. PUBLIC PROPERTIES (Data & State)
    // =========================================================
    #region PublicProperties

    /// <summary>
    /// Unique identifier for the list item (used when updating the collection).
    /// </summary>
    public string Id => AdapterName;

    #endregion
}
