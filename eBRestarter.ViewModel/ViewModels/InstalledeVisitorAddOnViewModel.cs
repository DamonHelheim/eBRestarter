using eBRestarter.Classes.InternetBrowser;
using eBRestarter.Classes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace eBRestarter.ViewModel.ViewModels
{
    public partial class InstalledeVisitorAddOnViewModel : BaseViewModel
    {

        private readonly Browser chromeBrowser = new Chrome(IWebLinks.CHROME_EBESUCHER_ADD_ON_LINK);
        private readonly Browser firefoxBrowser = new Firefox(IWebLinks.FIREFOX_EBESUCHER_ADD_ON_LINK);
        private readonly Browser edgeBrowser = new Edge(IWebLinks.EDGE_EBESUCHER_ADD_ON_LINK);
        private readonly Browser braveBrowser = new Brave(IWebLinks.CHROME_EBESUCHER_ADD_ON_LINK);

        private readonly string greenMark = "✓";
        private readonly string redMark = "✘";


        [ObservableProperty]
        public string tbl_ChromeIsInstalledText = string.Empty;

        [ObservableProperty]
        public string tbl_FirefoxIsInstalledText = string.Empty;

        [ObservableProperty]
        public string tbl_EdgeIsInstalledText = string.Empty;

        [ObservableProperty]
        public string tbl_BraveIsInstalledText = string.Empty;


        [ObservableProperty]
        public string tbl_ChromeIsInstalledTextForeground = string.Empty;

        [ObservableProperty]
        public string tbl_FirefoxIsInstalledTextForeground = string.Empty;

        [ObservableProperty]
        public string tbl_EdgeIsInstalledTextForeground = string.Empty;

        [ObservableProperty]
        public string tbl_BraveIsInstalledTextForeground = string.Empty;


        [ObservableProperty]
        public string tbl_AddOnChromeIsInstalledText = string.Empty;

        [ObservableProperty]
        public string tbl_AddOnFirefoxIsInstalledText = string.Empty;

        [ObservableProperty]
        public string tbl_AddOnEdgeIsInstalledText = string.Empty;

        [ObservableProperty]
        public string tbl_AddOnBraveIsInstalledText = string.Empty;


        [ObservableProperty]
        public string tbl_ChromeInstallInfoIcon = string.Empty;

        [ObservableProperty]
        public string tbl_FirefoxInstallInfoIcon = string.Empty;

        [ObservableProperty]
        public string tbl_EdgeInstallInfoIcon = string.Empty;

        [ObservableProperty]
        public string tbl_BraveInstallInfoIcon = string.Empty;


        [ObservableProperty]
        public string tbl_ChromeInstallInfoIconForeground = string.Empty;

        [ObservableProperty]
        public string tbl_FirefoxInstallInfoIconForeground = string.Empty;

        [ObservableProperty]
        public string tbl_EdgeInstallInfoIconForeground = string.Empty;

        [ObservableProperty]
        public string tbl_BraveInstallInfoIconForeground = string.Empty;


        [ObservableProperty]
        public bool? btnInstallChromeAddOnOverWebshopEnabled;

        [ObservableProperty]
        public bool? btnInstallEdgeAddOverWebshopEnabled;

        [ObservableProperty]
        public bool? btnInstallFirefoxAddOverWebshopEnabled;

        [ObservableProperty]
        public bool? btnInstallBraveAddOverWebshopEnabled;


        [ObservableProperty]
        public string btnInstallChromeAddOnOverWebshopContent = string.Empty;

        [ObservableProperty]
        public string btnInstallEdgeAddOverWebshopContent = string.Empty;

        [ObservableProperty]
        public string btnInstallFirefoxAddOverWebshopContent = string.Empty;

        [ObservableProperty]
        public string btnInstallBraveAddOverWebshopContent = string.Empty;


        public InstalledeVisitorAddOnViewModel() {

            UpdateWatcheVisitorValues();
        }


        [RelayCommand]
        public void ExecuteAddeVisitorChromeAddOn()
        {
            chromeBrowser.StartBrowser();
        }

        [RelayCommand]
        public void ExecuteAddeVisitorEdgeAddOn()
        {
            edgeBrowser.StartBrowser();
        }

        [RelayCommand]
        public void ExecuteAddeVisitorFirefoxAddOn()
        {
            firefoxBrowser.StartBrowser();          
        }

        [RelayCommand]
        public void ExecuteAddeVisitorBraveAddOn()
        {
            Debug.WriteLine(braveBrowser.ExtensionsDirectory + " " +braveBrowser.ExtensionsExist);

            //braveBrowser.StartBrowser();
        }


        public void UpdateWatcheVisitorValues()
        {
            Task.Run(() =>
            {

                while (true)
                {
                    try
                    {
                        CheckAddOnExist();
                        CheckBrowserExist();
                    }
                    catch (Exception)
                    {
                        
                    }

                    Thread.Sleep(1000);
                }

            });
        }


        private void CheckBrowserExist()
        {

            if (chromeBrowser.BrowserExist == true)
            {
                Tbl_ChromeIsInstalledText = "Chrome ist installiert";

                BtnInstallChromeAddOnOverWebshopEnabled = true;
            }
            else
            {
                Tbl_ChromeIsInstalledText = "Chrome ist nicht installiert";
                Tbl_ChromeIsInstalledTextForeground = "#BA224D";

                BtnInstallChromeAddOnOverWebshopEnabled = false;
            }

            if (edgeBrowser.BrowserExist == true)
            {
                Tbl_EdgeIsInstalledText = "Edge ist installiert";

                BtnInstallChromeAddOnOverWebshopEnabled = true;
            }
            else
            {
                Tbl_EdgeIsInstalledText = "Edge ist nicht installiert";
                Tbl_EdgeIsInstalledTextForeground = "#BA224D";

                BtnInstallEdgeAddOverWebshopEnabled = false;
            }

            if (firefoxBrowser.BrowserExist == true)
            {
                Tbl_FirefoxIsInstalledText = "Firefox ist installiert";

                BtnInstallFirefoxAddOverWebshopEnabled = true;
            }
            else
            {
                Tbl_FirefoxIsInstalledText = "Firefox ist nicht installiert";
                Tbl_FirefoxIsInstalledTextForeground = "#BA224D";

                BtnInstallFirefoxAddOverWebshopEnabled = false;
            }

            if (braveBrowser.BrowserExist == true)
            {
                Tbl_BraveIsInstalledText = "Brave ist installiert";

                BtnInstallBraveAddOverWebshopEnabled = true;
            }
            else
            {
                Tbl_BraveIsInstalledText = "Brave ist nicht installiert";
                Tbl_BraveIsInstalledTextForeground = "#BA224D";

                BtnInstallBraveAddOverWebshopEnabled = false;
            }

            //if (firefoxBrowser.BrowserExist == true || chromeBrowser.BrowserExist == true || edgeBrowser.BrowserExist)
            //{
            //}
            //else
            //{
            //}
        }


        private void CheckAddOnExist()
        {

            if (chromeBrowser.ExtensionsExist is true)
            {
                Tbl_AddOnChromeIsInstalledText = "Add-on ist installiert";

                Tbl_ChromeInstallInfoIcon = greenMark;
                Tbl_ChromeInstallInfoIconForeground = "#7ED422";

                BtnInstallChromeAddOnOverWebshopContent = "Add-on über Chrome web store deinstallieren";
            }
            else
            {
                Tbl_AddOnChromeIsInstalledText = "Add-on ist nicht installiert";

                Tbl_ChromeInstallInfoIcon = redMark;
                Tbl_ChromeInstallInfoIconForeground = "#E40E87";

                BtnInstallChromeAddOnOverWebshopContent = "Add-on über Chrome web store installieren";
            }


            if (edgeBrowser.ExtensionsExist is true)
            {
                Tbl_AddOnEdgeIsInstalledText = "Add-on ist installiert";

                Tbl_EdgeInstallInfoIcon = greenMark;
                Tbl_EdgeInstallInfoIconForeground = "#7ED422";

                BtnInstallEdgeAddOverWebshopContent = "Add-on über Edge web store deinstallieren";
            }
            else
            {
                Tbl_AddOnEdgeIsInstalledText = "Add-on ist nicht installiert";

                Tbl_EdgeInstallInfoIcon = redMark;
                Tbl_EdgeInstallInfoIconForeground = "#E40E87";

                BtnInstallEdgeAddOverWebshopContent = "Add-on über Edge web store installieren";
            }

            if (firefoxBrowser.ExtensionsExist)
            {
                Tbl_AddOnFirefoxIsInstalledText = "Add-on ist installiert";

                Tbl_FirefoxInstallInfoIcon = greenMark;
                Tbl_FirefoxInstallInfoIconForeground = "#7ED422";

                BtnInstallFirefoxAddOverWebshopContent = "Add-on über Firefox Erweiterungen deinstallieren";
            }
            else
            {
                Tbl_AddOnFirefoxIsInstalledText = "Add-on ist nicht installiert";

                Tbl_FirefoxInstallInfoIcon = redMark;
                Tbl_FirefoxInstallInfoIconForeground = "#E40E87";

                BtnInstallFirefoxAddOverWebshopContent = "Add-on über Firefox Erweiterungen installieren";
            }

            if (braveBrowser.ExtensionsExist is true)
            {
                Tbl_AddOnBraveIsInstalledText = "Add-on ist installiert";

                Tbl_BraveInstallInfoIcon = greenMark;
                Tbl_BraveInstallInfoIconForeground = "#7ED422";

                BtnInstallBraveAddOverWebshopContent = "Add-on über Chrome web store deinstallieren";
            }
            else
            {
                Tbl_AddOnBraveIsInstalledText = "Add-on ist nicht installiert";

                Tbl_BraveInstallInfoIcon = redMark;
                Tbl_BraveInstallInfoIconForeground = "#E40E87";

                BtnInstallBraveAddOverWebshopContent = "Add-on über Chrome web store installieren";
            }

        }
    }
}
