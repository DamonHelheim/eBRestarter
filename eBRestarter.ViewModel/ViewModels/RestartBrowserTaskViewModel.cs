using eBRestarter.Classes.EBFiles;
using eBRestarter.Classes.EBInterfaces;
using eBRestarter.Classes.EBScheduler;
using eBRestarter.Classes.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text.RegularExpressions;

namespace eBRestarter.ViewModel.ViewModels
{
    public partial class RestartBrowserTaskViewModel : BaseViewModel
    {
        [ObservableProperty]
        public string username = string.Empty;

        [ObservableProperty]
        public string choosenBrowser = string.Empty;

        [ObservableProperty]
        public string countDownTimer = string.Empty;

        [ObservableProperty]
        public string deleteDate = string.Empty;

        [ObservableProperty]
        public string tbtnStartTimerSchudelerContent = string.Empty;

        [ObservableProperty]
        public string tbtnStartTimerSchudelerBackground = string.Empty;

        [ObservableProperty]
        public bool toggleButtonClicked;

        private delegate void ShowCountDownTimer(int seconds, CountdownVisuals countdownVisuals);

        private readonly IEBXMLFile eBXMLFileManagement = new EBXMLFileManagement();

        public RestartBrowserTaskViewModel() {

            TbtnStartTimerSchudelerContent = "Start";
            TbtnStartTimerSchudelerBackground = "#FF0DB290";

            UpdateRestartTaskValuesTask();

            EBXMLFileService eBFileService = new(eBXMLFileManagement);          

            Username = eBFileService.GetXMLAttributeInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.UsernameTag, IConfigConstants.UsernameTag_Name_Attribut);
            ChoosenBrowser = eBFileService.GetXMLAttributeInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.BrowserTag, IConfigConstants.BrowserTag_Selected_Attribut);

        }

        [RelayCommand]
        private void ExecuteStartTimerScheduler(object value)
        {
            ToggleButtonClicked = (bool)value;
            EBXMLFileService eBXMLFileService = new(eBXMLFileManagement);
            string Browser = eBXMLFileService.GetXMLAttributeInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.BrowserTag, IConfigConstants.BrowserTag_Selected_Attribut);

            if (Regex.IsMatch(Browser, IRegexPattern.BrowserSelection))
            {
                if (ToggleButtonClicked is true)
                {
                    TbtnStartTimerSchudelerContent = "Stop";
                    TbtnStartTimerSchudelerBackground = "#b30c2e";
                }
                else
                {
                    TbtnStartTimerSchudelerContent = "Start";
                    TbtnStartTimerSchudelerBackground = "#FF0DB290";
                }
            } else
            {
                ToggleButtonClicked = false;
                //Do nothing
            }

            AppDomain.CurrentDomain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", TimeSpan.FromMilliseconds(100));
        }


        private void UpdateRestartTaskValuesTask()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    DeleteDate = RestartBrowserTaskMediatorViewModel.BrowserContentDeleteDate + " um 0 Uhr Mitternacht";

                    while (RestartBrowserTaskMediatorViewModel.TriggerUsernameMediator == true)
                    {
                        Username = RestartBrowserTaskMediatorViewModel.UsernameMediator;

                        RestartBrowserTaskMediatorViewModel.TriggerUsernameMediator = false;
                    }

                    if (RestartBrowserTaskMediatorViewModel.TriggerChossenBrowserMediator == true)
                    {
                        ChoosenBrowser = RestartBrowserTaskMediatorViewModel.ChossenBrowserMediator;

                        RestartBrowserTaskMediatorViewModel.TriggerChossenBrowserMediator = false;
                    }

                    Thread.Sleep(175);
                }
            }
          );
        }

        //private readonly CountdownVisuals countdownVisuals = new();

        //private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        //private delegate Task ShowCountDownTimer(int seconds, CountdownVisuals countdownVisuals);

        //private ShowCountDownTimer showCountDownTimer = (seconds, countdownVisuals) =>

        //Task.Run(() =>
        //{
        //    //CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        //    cancellationTokenSource = new CancellationTokenSource();
        //    CancellationToken cancellationToken = cancellationTokenSource.Token;

        //    countdownVisuals.SetSeconds = seconds;

        //    while (!cancellationToken.IsCancellationRequested)
        //    {
        //        //CountDownTimer = countdownVisuals.GetTimeHourAndMinutesAndSeconds();
        //        Thread.Sleep(1000);
        //    }
        //}
        //  );

        //private void MainTimertest(int seconds, CountdownVisuals countdownVisuals)
        //{           
        //    Task.Run(() =>
        //    {    
        //        cancellationTokenSource = new CancellationTokenSource();
        //        CancellationToken cancellationToken = cancellationTokenSource.Token;

        //        countdownVisuals.SetSeconds = seconds;

        //        while (!cancellationToken.IsCancellationRequested) {

        //            CountDownTimer = countdownVisuals.ShowCurrentTime();

        //            Thread.Sleep(1000);
        //        }               
        //    }
        //  ); 
        //}
        //[RelayCommand]
        //private void ExecuteStartTimerScheduler(object value)
        //{
        //    bool a = (bool)value;

        //    if(a is true)
        //    {
        //        TbtnStartTimerSchudelerContent = "Stop";
        //        TbtnStartTimerSchudelerBackground = "#b30c2e";

        //        //Task task = showCountDownTimer(5400, Main_CDV, CountDownTimer);

        //        //ShowCountDownTimer PreperationTimer = new ShowCountDownTimer(MainTimer);


        //        //PreperationTimer(20, countdownVisuals);

        //        Debug.WriteLine("Reached");

        //        cancellationTokenSource.Cancel();

        //        //ShowCountDownTimer BrowserTimer = new ShowCountDownTimer(MainTimer);


        //        //BrowserTimer(5600, countdownVisuals);

        //    } else
        //    {
        //        cancellationTokenSource.Cancel();
        //        TbtnStartTimerSchudelerContent = "Start";
        //        TbtnStartTimerSchudelerBackground = "#FF0DB290";
        //    }

        //}

    }
}
