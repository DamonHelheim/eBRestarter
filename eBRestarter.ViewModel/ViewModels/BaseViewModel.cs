using CommunityToolkit.Mvvm.ComponentModel;

namespace eBRestarter.ViewModel.ViewModels
{
    public partial class BaseViewModel :  ObservableObject
    {

        public delegate bool DelShowInformationWindowType(string iMessage);
        public DelShowInformationWindowType? DelShowInformationWindow { get; set; }

        [ObservableProperty]
        string titel = string.Empty;

    }
}
