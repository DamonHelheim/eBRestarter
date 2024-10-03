using CommunityToolkit.Mvvm.ComponentModel;

namespace eBRestarter.Model.Models
{
    public partial class WindowsInformation : ObservableObject
    {
        [ObservableProperty]
        string currentOSBuildVersion = string.Empty;

        [ObservableProperty]
        string currentOSVersion = string.Empty;

        [ObservableProperty]
        string currentEdition = string.Empty;
    }
}
