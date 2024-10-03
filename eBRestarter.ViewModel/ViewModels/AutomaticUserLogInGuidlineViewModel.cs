using eBRestarter.Classes.OperatingSystem;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace eBRestarter.ViewModel.ViewModels
{
    public partial class AutomaticUserLogInGuidlineViewModel : BaseViewModel
    {

        [ObservableProperty]
        public bool canExecuteButtonTrigger;

        #pragma warning disable CA1416
        private readonly WindowsOS windowsOS = new();

        public AutomaticUserLogInGuidlineViewModel()
        {

        }

        [RelayCommand(CanExecute = nameof(CanExcecuteUserLogoutWindow))]
        private void ExcecuteUserLoginWindow(object value)
        {
            _ = Process.Start("Netplwiz.exe");
        }


        public static bool CanExcecuteUserLogoutWindow(object value) {

            bool boolValue = (bool)value;

            if (boolValue is true)
            {
                return true;

            } else {

                return false;
            }         
        }


        [RelayCommand]
        private void ExcecuteActivateAutoLogin(object value)
        {
            bool boolValue = (bool)value;

            if(boolValue is true)
            {
                windowsOS.ActivatePossibilityUserAutologonForWindows();

            } else
            {
                windowsOS.DeactivatePossibilityUserAutologonForWindows();
            }         
        }
    }
}
