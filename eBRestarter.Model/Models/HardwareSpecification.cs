using CommunityToolkit.Mvvm.ComponentModel;

namespace eBRestarter.Model.Models
{
    public partial class HardwareSpecification : ObservableObject
    {
        public HardwareSpecification() { }

        [ObservableProperty]
        public string processor = string.Empty;

        [ObservableProperty]
        public string graphics = string.Empty;

        [ObservableProperty]
        public string ram = string.Empty;
    }
}
