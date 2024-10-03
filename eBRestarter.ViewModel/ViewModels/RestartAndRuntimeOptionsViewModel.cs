using eBRestarter.Classes.EBFiles;
using eBRestarter.Classes.EBInterfaces;
using eBRestarter.Classes.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace eBRestarter.ViewModel.ViewModels
{
    public partial class RestartAndRuntimeOptionsViewModel : BaseViewModel
    {

        [ObservableProperty]
        public int browser_waiting_time_min;

        [ObservableProperty]
        public int browser_waiting_time_max;

        [ObservableProperty]
        public int browser_waiting_runtime_min;

        [ObservableProperty]
        public int browser_waiting_runtime_max;

        [ObservableProperty]
        public int sliderBrowserWaitingTimeToStartValue;

        [ObservableProperty]
        public int sliderBrowserRuntimeValue;

        private readonly IEBXMLFile eBXMLFileManagement = new EBXMLFileManagement();

        public RestartAndRuntimeOptionsViewModel(){

            LoadSettings();
        }


        private void LoadSettings()
        {

            Browser_waiting_time_min = 20;
            Browser_waiting_time_max = 60;

            Browser_waiting_runtime_min = 1;
            Browser_waiting_runtime_max = 12;

            EBXMLFileService eBFileService = new(eBXMLFileManagement);

            SliderBrowserRuntimeValue = eBFileService.GetXMLTagValueInConfig<int>(ISystemPaths.File_Path_eBRestarter_Settings, "Runtime");
            SliderBrowserWaitingTimeToStartValue = eBFileService.GetXMLTagValueInConfig<int>(ISystemPaths.File_Path_eBRestarter_Settings, "Rest");
        }

        public void Browser_runtime(string runtime)
        {
            EBXMLFileService eBFileService = new(eBXMLFileManagement);

            eBFileService.WriteXMLTagValueInConfig("Runtime", runtime);
        }

        public void Browser_waiting_time(string waitingTime)
        {
            EBXMLFileService eBFileService = new(eBXMLFileManagement);

            eBFileService.WriteXMLTagValueInConfig("Rest", waitingTime);
        }

    }
}
