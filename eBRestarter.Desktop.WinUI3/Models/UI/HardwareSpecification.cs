using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Desktop.WinUI3.Models.UI
{
    /// <summary>
    /// UI display model for hardware specification (processor, graphics, RAM).
    /// </summary>
    public partial class HardwareSpecification : ObservableObject
    {
        // =========================================================
        // 1. OBSERVABLE PROPERTIES (MVVM State)
        // =========================================================
        #region ObservableProperties

        [ObservableProperty] public partial string Processor { get; set; } = string.Empty;
        [ObservableProperty] public partial string Graphics { get; set; } = string.Empty;
        [ObservableProperty] public partial string Ram { get; set; } = string.Empty;

        #endregion
    }
}
