using eBRestarter.Classes.InternetBrowser;
using CommunityToolkit.Mvvm.ComponentModel;


namespace eBRestarter.ViewModel.ViewModels
{
    public partial class BrowserViewModel : BaseViewModel
    {

        private readonly Browser chromeBrowser = new Chrome();
        private readonly Browser firefoxBrowser = new Firefox();
        private readonly Browser edgeBrowser = new Edge();

        private const string setForegroundColorGreen = "#7ED422";
        private const string setForegroundColorRed = "#E40E87";
        private const string greenMark = "✓";
        private const string redMark = "✘";


        [ObservableProperty]
        public string edgeVersion = string.Empty;

        [ObservableProperty]
        public string chromeVersion = string.Empty;

        [ObservableProperty]
        public string firefoxVersion = string.Empty;


        [ObservableProperty]
        private string chooseButtonChromeIsVisible = string.Empty;

        [ObservableProperty]
        private string chooseButtonFirefoxIsVisible = string.Empty;

        [ObservableProperty]
        private string chooseButtonEdgeIsVisible = string.Empty;

        [ObservableProperty]
        private string downloadButtonChromeIsVisible = string.Empty;

        [ObservableProperty]
        private string downloadButtonFirefoxIsVisible = string.Empty;

        [ObservableProperty]
        private string downloadButtonEdgeIsVisible = string.Empty;


        [ObservableProperty]
        private string btnDeleteBrowserContentIsEnabled = string.Empty;

        [ObservableProperty]
        private string btnInstalleBesucherAddOnIsEnabled = string.Empty;


        [ObservableProperty]
        private string browserExistChrome = string.Empty;

        [ObservableProperty]
        private string browserExistFirefox = string.Empty;

        [ObservableProperty]
        private string browserExistEdge = string.Empty;


        [ObservableProperty]
        private string textForegroundColorChromeExist = string.Empty;

        [ObservableProperty]
        private string textForegroundColorFirefoxExist = string.Empty;

        [ObservableProperty]
        private string textForegroundColorEdgeExist = string.Empty;

        [ObservableProperty]
        private string tbl_NoBrowserInstalledIsVisible = string.Empty;


        public BrowserViewModel()
        {
            UpdateWatchBrowserValues();
        }

        private void UpdateWatchBrowserValues()
        {
            Task.Run(() => {

                while (true)
                {
                    try
                    {
                        if (chromeBrowser.BrowserVersion != null)
                        {
                            ChromeVersion                   = "Version: " + chromeBrowser.BrowserVersion;
                            TextForegroundColorChromeExist  = setForegroundColorGreen;
                            BrowserExistChrome              = greenMark;

                            ChooseButtonChromeIsVisible     = "Visible";
                            DownloadButtonChromeIsVisible   = "Hidden";
                        }
                        else
                        {
                            ChromeVersion                   = "Nicht installiert";
                            TextForegroundColorChromeExist  = setForegroundColorRed;
                            BrowserExistChrome              = redMark;

                            ChooseButtonChromeIsVisible     = "Hidden";
                            DownloadButtonChromeIsVisible   = "Visible";
                        }

                        if (edgeBrowser.BrowserVersion != null && edgeBrowser.BrowserExist is true)
                        {
                            EdgeVersion                     = "Version: " + edgeBrowser.BrowserVersion;
                            TextForegroundColorEdgeExist    = setForegroundColorGreen;
                            BrowserExistEdge                = greenMark;

                            ChooseButtonEdgeIsVisible      = "Visible";
                            DownloadButtonEdgeIsVisible     = "Hidden";
                        }
                        else
                        {
                            EdgeVersion                     = "Nicht installiert";
                            TextForegroundColorEdgeExist    = setForegroundColorRed;
                            BrowserExistEdge                = redMark;

                            ChooseButtonEdgeIsVisible      = "Hidden";
                            DownloadButtonEdgeIsVisible     = "Visible";
                        }

                        if (firefoxBrowser.BrowserVersion != null)
                        {
                            FirefoxVersion                  = "Version: " + firefoxBrowser.BrowserVersion;

                            TextForegroundColorFirefoxExist = "#7ED422";
                            BrowserExistFirefox             = greenMark;
                            
                            ChooseButtonFirefoxIsVisible    = "Visible";
                            DownloadButtonFirefoxIsVisible  = "Hidden";
                        }
                        else
                        {
                            FirefoxVersion                  = "Nicht installiert";
                            TextForegroundColorFirefoxExist = setForegroundColorRed;
                            BrowserExistFirefox             = redMark;

                            ChooseButtonFirefoxIsVisible    = "Hidden";
                            DownloadButtonFirefoxIsVisible  = "Visible";
                        }


                        if (firefoxBrowser.BrowserExist is true || chromeBrowser.BrowserExist is true || edgeBrowser.BrowserExist is true)
                        {
                            BtnDeleteBrowserContentIsEnabled    = "True";
                            BtnInstalleBesucherAddOnIsEnabled   = "True";
                            Tbl_NoBrowserInstalledIsVisible     = "Hidden";
                        }
                        else
                        {  
                            BtnDeleteBrowserContentIsEnabled    = "False";
                            BtnInstalleBesucherAddOnIsEnabled   = "False";
                            Tbl_NoBrowserInstalledIsVisible     = "Visible";
                        }
                    }
                    catch (Exception)
                    {

                    }

                    Thread.Sleep(1000);
                }

            });
        }
    }
}
