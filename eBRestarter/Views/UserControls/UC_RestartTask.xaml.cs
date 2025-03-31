using eBRestarter.Classes.EBInterfaces;
using eBRestarter.Classes.Services;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using eBRestarter.Classes.InternetBrowser;
using eBRestarter.Classes.OperatingSystem;
using eBRestarter.Views.Windows;
using System.Text.RegularExpressions;
using System.Windows.Automation;
using eBRestarter.Classes.EBFiles;
using eBRestarter.ViewModel.ViewModels;
using eBRestarter.Classes.EBScheduler;


namespace eBRestarter.Views.UserControls
{
    public partial class UC_RestartTask : UserControl, IRegexPattern, IConfigConstants
    {
        private readonly System.Windows.Threading.DispatcherTimer? mainTimer = new ();
        private readonly System.Windows.Threading.DispatcherTimer? BrowserStartsInTimer = new ();
        private readonly System.Windows.Threading.DispatcherTimer? StartRestartTimer = new ();

        private readonly CountdownVisuals? mainTimerVisuals = new();
        private readonly CountdownVisuals? secondsTimerBrowserStartsInVisuals = new ();
        private readonly CountdownVisuals? third_CDV_StartRestartTimer = new ();

        private Browser? firefoxBrowser;
        private Browser? chromeBrowser;
        private Browser? edgeBrowser;
        private Browser? braveBrowser;

        public string choosenBrowser = string.Empty;
        public string username = string.Empty;

        private const int seconds = 1;
        private const int StartRestartTimerSeconds = 5;
        public int mainCounterSeconds = 0;
        public int browserStartsInSeconds = 0;
        
        public WindowsOS windowsOS = new();

        private readonly DateStampService days = new();

        private readonly IEBXMLFile eBXMLFileManagement = new EBXMLFileManagement();

        public RestartBrowserTaskViewModel RestartBrowserTaskViewModel { get; } = new();


        public UC_RestartTask()
        {
            InitializeComponent();

            InitializeCountdownTimer();
            InitializeObjects();

            this.DataContext = RestartBrowserTaskViewModel;

            EBXMLFileService eBXMLFileService   = new(eBXMLFileManagement);
            mainCounterSeconds                  = eBXMLFileService.GetXMLTagValueInConfig<int>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.BrowserRuntimeTag) * 60 * 60;
            browserStartsInSeconds              = eBXMLFileService.GetXMLTagValueInConfig<int>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.BrowserRestTag);

            string StartRestarterWithProgrammStart = eBXMLFileService.GetXMLTagValueInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.StartRestarterWithProgrammStartTag).ToLower();
            if (StartRestarterWithProgrammStart.Equals("true")) { StartRestarterTaskWithStart(); }           
        }

        private void Tbtn_StartTimerSchudeler_Click(object sender, RoutedEventArgs e) {

            EBXMLFileService eBXMLFileService = new(eBXMLFileManagement);
            string browser = eBXMLFileService.GetXMLAttributeInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.BrowserTag, IConfigConstants.BrowserTag_Selected_Attribut);
            string username = eBXMLFileService.GetXMLAttributeInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.UsernameTag, IConfigConstants.UsernameTag_Name_Attribut);

            if (string.IsNullOrEmpty(username) || string.IsNullOrWhiteSpace(username))
            {
                tbtn_StartTimerSchudeler.IsChecked = false;

                _ = MessageBox.Show("Es wurde noch kein Username ausgewählt!", "Nutzername", MessageBoxButton.OK, MessageBoxImage.Error);

            } else if (Regex.IsMatch(browser, IRegexPattern.BrowserSelection))  {

                if (tbtn_StartTimerSchudeler.IsChecked is true)
                {
                    StartRestartTimerTask();
                }
                else
                {
                    StopRestartTimerTask();
                }

                AppDomain.CurrentDomain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", TimeSpan.FromMilliseconds(100));
            }
            else
            {
                tbtn_StartTimerSchudeler.IsChecked = false;
                _ = MessageBox.Show("Es wurde kein Browser ausgewählt!", "Browser", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
      
        private void StartRestarterTaskWithStart()
        {        
            RestartBrowserTaskViewModel.TbtnStartTimerSchudelerContent = "Stop";
            RestartBrowserTaskViewModel.TbtnStartTimerSchudelerBackground = "#b30c2e";
            tbtn_StartTimerSchudeler.IsChecked = true;         

            EBXMLFileService eBXMLFileService = new(eBXMLFileManagement);
            string Browser = eBXMLFileService.GetXMLAttributeInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.BrowserTag, IConfigConstants.BrowserTag_Selected_Attribut);
            string username = eBXMLFileService.GetXMLAttributeInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.UsernameTag, IConfigConstants.UsernameTag_Name_Attribut);

            if (string.IsNullOrEmpty(username) || string.IsNullOrWhiteSpace(username))
            {
                tbtn_StartTimerSchudeler.IsChecked = false;

                MessageBoxResult result = MessageBox.Show("Es wurde noch kein Username auswgewählt!", "Nutzername", MessageBoxButton.OK, MessageBoxImage.Error);

            } else if (Regex.IsMatch(Browser, IRegexPattern.BrowserSelection)) {

                if (tbtn_StartTimerSchudeler.IsChecked is true)
                {
                    StartRestartTimerTask();
                }
                else
                {
                    StopRestartTimerTask();
                }
            }
            else
            {
                tbtn_StartTimerSchudeler.IsChecked = false;
                MessageBoxResult result = MessageBox.Show("Es wurde kein Browser ausgewählt!", "Browser", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public void CloseProcess(string name)
        {
            foreach (Process process in Process.GetProcessesByName(name))
            {
                if (process.MainWindowHandle == IntPtr.Zero) // some have no UI
                {
                    continue;
                }

                AutomationElement element = AutomationElement.FromHandle(process.MainWindowHandle);

                if (element != null)
                {

                    ((WindowPattern)element.GetCurrentPattern(WindowPattern.Pattern)).Close();

                }
            }
        }

        private void InitializeCountdownTimer()
        {
            StartRestartTimer!.Tick      += StartRestartTimer_Tick!;
            mainTimer!.Tick              += MainTimer_Tick!;
            BrowserStartsInTimer!.Tick   += BrowserStartsInTimer_Tick!;
        }

        private void InitializeObjects()
        {
            chromeBrowser = new Chrome();
            firefoxBrowser = new Firefox();
            edgeBrowser = new Edge();
            braveBrowser = new Brave();
        }


        public void StartRestartTimerTask()
        {
            EBXMLFileService eBFileService = new(eBXMLFileManagement);

            mainCounterSeconds = eBFileService.GetXMLTagValueInConfig<int>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.BrowserRuntimeTag) * 60 * 60;

            browserStartsInSeconds = eBFileService.GetXMLTagValueInConfig<int>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.BrowserRestTag);

            third_CDV_StartRestartTimer!.SetSeconds = StartRestartTimerSeconds;

            StartRestartTimer!.Interval = new TimeSpan(0, 0, seconds);

            StartRestartTimer.Start();
        }


        private void MainCountdownTimerStart()
        {
            mainTimerVisuals!.SetSeconds = mainCounterSeconds;
            mainTimer!.Interval = new TimeSpan(0, 0, seconds);
            StartBrowser();

            mainTimer.Start();
            
            tbl_CurrentStatusInfo.Text = "Der Countdown Timer läuft.";
        }

        public void StopRestartTimerTask()
        {
            StartRestartTimer!.Stop();
            mainTimer!.Stop();
            BrowserStartsInTimer!.Stop();

            StopBrowser();

            tbl_CurrentStatusInfo.Text = "Browser Restart wurde gestoppt.";
            tbl_Timer.Text = "-";
        }

        private void StartRestartTimer_Tick(object sender, EventArgs e)
        {
            if (third_CDV_StartRestartTimer!.GetSeconds == 0)
            {
                tbl_CurrentStatusInfo.Text = "Starte Browser in: " + third_CDV_StartRestartTimer.GetTimeSeconds();

                StartRestartTimer!.Stop();
                MainCountdownTimerStart();
            }
            else
            {
                tbl_CurrentStatusInfo.Text = "Starte Browser in: " + third_CDV_StartRestartTimer.GetTimeSeconds();
            }
        }
        private readonly IEBFile eBFileManagement = new EBFileManagement();
        private void MainTimer_Tick(object sender, EventArgs e)
        {
            if (mainTimerVisuals!.GetSeconds == 0)
            {
                StopBrowser();
                ResetMainTimer();
                BrowserStartsInCountdownTimerStart();
            }
            else
            {
                StartNewBrowserSession();

                if ((days.ReachDateDeleteProcess() && DateTime.Now.ToString("t").Equals("00:00")) is true){

                    StopBrowser();
                    ResetMainTimer();
                    UpdateDeleteDateGUI();
                    BrowserDeleteProcess();

                }
                else
                {
                    tbl_Timer.Text = mainTimerVisuals.GetTimeHourAndMinutesAndSeconds();
                }
            }
        }

        private void BrowserDeleteProcess()
        {
            tbl_CurrentStatusInfo.Text = "Lösche Cookies und Internetcache...";
            tbl_Timer.Text = "-";

            BrowserDeleteProcessInterfaceService bPIService = new();

            if (MatchFirefoxBrowser() == true)
            {
                var dlb = new DeleteBrowserContentWindow(bPIService.ListContainer, bPIService.BrowserName!, bPIService.BrowserIconPath!, bPIService.FirefoxCookiePath!, true)
                {
                    GetFirefoxCookiePath = bPIService.FirefoxCookiePath!
                };

                dlb.ShowDialog();
            }
            else
            {
                _ = new DeleteBrowserContentWindow(bPIService.ListContainer, bPIService.BrowserName!, bPIService.BrowserIconPath!, "", true).ShowDialog();
            }

            BrowserStartsInCountdownTimerStart();
        }

        private void UpdateDeleteDateGUI()
        {
            EBFileService eBFileService = new(eBFileManagement);
            EBFileAccess.AccessFile!(IFileNames.DELETEDATE);
            RestartBrowserTaskMediatorViewModel.BrowserContentDeleteDate = eBFileService.ReadFile(ISystemPaths.File_Path_Time_Stamp_Delete_Process);
            EBFileAccess.LockFile!(IFileNames.DELETEDATE);
        }

        private void BrowserStartsInTimer_Tick(object sender, EventArgs e)
        {
            if (secondsTimerBrowserStartsInVisuals!.GetSeconds == 0)
            {
                tbl_Timer.Text = secondsTimerBrowserStartsInVisuals.GetTimeSeconds();
                BrowserStartsInTimer!.Stop();
                MainCountdownTimerStart();
            }
            else
            {
                tbl_Timer.Text = secondsTimerBrowserStartsInVisuals.GetTimeSeconds();
            }
        }

        private void BrowserStartsInCountdownTimerStart()
        {
            tbl_CurrentStatusInfo.Text = "Warte auf Neustart, Browser startet in:";

            secondsTimerBrowserStartsInVisuals!.SetSeconds = browserStartsInSeconds;

            BrowserStartsInTimer!.Interval = new TimeSpan(0, 0, seconds);
            BrowserStartsInTimer.Start();
        }

        private void ResetMainTimer()
        {
            tbl_Timer.Text = mainTimerVisuals!.GetTimeHourAndMinutesAndSeconds();
            mainTimer!.Stop();
        }
        private void StartBrowser()
        {
            EBXMLFileService eBFileService = new(eBXMLFileManagement);

            username = eBFileService.GetXMLAttributeInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.UsernameTag, IConfigConstants.UsernameTag_Name_Attribut);

            if (MatchChromeBrowser() == true)
            {
                chromeBrowser = new Chrome(IWebLinks.EV_SURFLINK, username);
                chromeBrowser.StartBrowser();
            }
            else if (MatchFirefoxBrowser() == true)
            {
                firefoxBrowser = new Firefox(IWebLinks.EV_SURFLINK, username);
                firefoxBrowser.StartBrowser();
            }

            else if (MatchEdgeBrowser() == true)
            {
                edgeBrowser = new Edge(IWebLinks.EV_SURFLINK, username);
                edgeBrowser.StartBrowser();
            }
            else if (MatchBraveBrowser() == true)
            {
                braveBrowser = new Edge(IWebLinks.EV_SURFLINK, username);
                braveBrowser.StartBrowser();
            }
        }


        private void StartNewBrowserSession() //Start new browser session if a browser is switched at runtime
        {
            EBXMLFileService eBFileService = new(eBXMLFileManagement);

            bool checkBrowserAliveRoutine = eBFileService.GetXMLTagValueInConfig<bool>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.CheckBrowserAliveRoutineTag);

            if (checkBrowserAliveRoutine is true)
            {
                if (MatchChromeBrowser() == true) //If the chrome browser has been selected during runtime, then it will be closed and it will be checked if there is another firefox instance open.
                {
                    if (windowsOS.ProcessAlive("chrome") == false)
                    {
                        StartNewBrowserSessionValues(chromeBrowser!, "Browser wurde geschlossen, dieser startet in:");
                    }
                }

                if (MatchFirefoxBrowser() == true)
                {
                    if (windowsOS.ProcessAlive("firefox") == false)
                    {
                        StartNewBrowserSessionValues(firefoxBrowser!, "Browser wurde geschlossen, dieser startet in:");
                    }
                }

                if (MatchEdgeBrowser() == true)
                {
                    if (windowsOS.ProcessAlive("msedge") == false)
                    {
                        StartNewBrowserSessionValues(edgeBrowser!, "Browser wurde geschlossen, dieser startet in:");
                    }
                }

                if (MatchEdgeBrowser() == true)
                {
                    if (windowsOS.ProcessAlive("brave") == false)
                    {
                        StartNewBrowserSessionValues(edgeBrowser!, "Browser wurde geschlossen, dieser startet in:");
                    }
                }
            }
        }

        private void StartNewBrowserSessionValues(Browser browser, string text)
        {
            browser.CloseBrowser();
            mainTimer!.Stop();
            BrowserStartsInCountdownTimerStart();
            tbl_CurrentStatusInfo.Text = text;

        }

        public void StopBrowser()
        {
            if (MatchChromeBrowser() == true)   { CloseProcess("chrome");          /*chromeBrowser!.CloseBrowser();*/  }

            if (MatchFirefoxBrowser() == true)  { CloseProcess("firefox");         /*firefoxBrowser!.CloseBrowser();*/ }

            if (MatchEdgeBrowser() == true)     { windowsOS.StopProcess("msedge"); /*edgeBrowser!.CloseBrowser();*/    }

            if (MatchBraveBrowser() == true)    { CloseProcess("brave");          /*chromeBrowser!.CloseBrowser();*/   }
        }

        public bool MatchChromeBrowser()
        {
            EBXMLFileService eBFileService = new(eBXMLFileManagement);
            choosenBrowser = eBFileService.GetXMLAttributeInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.BrowserTag, IConfigConstants.BrowserTag_Selected_Attribut);

            if (choosenBrowser.Equals("Chrome"))
            {
                return true;
            }
            else { return false; }
        }

        public bool MatchFirefoxBrowser()
        {
            EBXMLFileService eBFileService = new(eBXMLFileManagement);
            choosenBrowser = eBFileService.GetXMLAttributeInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.BrowserTag, IConfigConstants.BrowserTag_Selected_Attribut);

            if (choosenBrowser.Equals("Firefox"))
            {
                return true;
            }
            else { return false; }
        }

        public bool MatchEdgeBrowser()
        {
            EBXMLFileService eBFileService = new(eBXMLFileManagement);
            choosenBrowser = eBFileService.GetXMLAttributeInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.BrowserTag, IConfigConstants.BrowserTag_Selected_Attribut);

            if (choosenBrowser.Equals("Edge"))
            {
                return true;
            }
            else { return false; }
        }

        public bool MatchBraveBrowser()
        {
            EBXMLFileService eBFileService = new(eBXMLFileManagement);
            choosenBrowser = eBFileService.GetXMLAttributeInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.BrowserTag, IConfigConstants.BrowserTag_Selected_Attribut);

            if (choosenBrowser.Equals("Brave"))
            {
                return true;
            }
            else { return false; }
        }
    }
}
