using CommunityToolkit.Mvvm.ComponentModel;

namespace eBRestarter.Desktop.WinUI3.ObjectArchetypes.ObservableModel;

/// <summary>
/// UI display model for hardware specification (processor, graphics, RAM).
/// </summary>
public sealed partial class HardwareSpecificationModel : ObservableObject
{
    [ObservableProperty] public partial string Processor { get; set; } = string.Empty;
    [ObservableProperty] public partial string Graphics { get; set; } = string.Empty;
    [ObservableProperty] public partial string Ram { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="HardwareSpecificationModel"/> class.
    /// </summary>
    /// <param name="processor">The processor description string.</param>
    /// <param name="graphics">The graphics adapter description string.</param>
    /// <param name="ram">The total RAM capacity description string.</param>
    public HardwareSpecificationModel(string processor, string graphics, string ram)
    {
        Processor = processor;
        Graphics = graphics;
        Ram = ram;
    }
}
