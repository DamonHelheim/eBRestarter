using CommunityToolkit.Mvvm.ComponentModel;

namespace eBRestarter.ViewModel.ViewModels
{
    public partial class DeleteBrowserContentDateViewModel : BaseViewModel
    {

        [ObservableProperty]
        public string textInfo = string.Empty;

        [ObservableProperty]
        public string deleteDate = string.Empty;

        [ObservableProperty]
        public string isActivated = string.Empty;

        [ObservableProperty]
        public string imagePath = string.Empty;

        public DeleteBrowserContentDateViewModel()
        {
            UpdateRestartTaskValuesTask();
        }

        private void UpdateRestartTaskValuesTask()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    if (RestartBrowserTaskMediatorViewModel.BrowserContentDeleteDate is "false") {

                        IsActivated = "Deaktiviert";
                        TextInfo = "Nicht ausgewählt";
                        DeleteDate = "";
                        ImagePath = @"\Resources\Images\Icons\Intersection\deactivate.png";
                    }
                    else
                    {                       
                        IsActivated = "Aktiviert";
                        TextInfo = "Nächster Löschvorgang:";
                        ImagePath = @"\Resources\Images\Icons\Intersection\plus.png";
                        DeleteDate = RestartBrowserTaskMediatorViewModel.BrowserContentDeleteDate + " um 0 Uhr Mitternacht";
                    }

                    Thread.Sleep(175);
                }
            });
        }
    }
}
