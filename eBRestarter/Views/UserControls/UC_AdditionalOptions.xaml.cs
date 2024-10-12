using eBRestarter.Classes.EBFiles;
using eBRestarter.Classes.EBInterfaces;
using eBRestarter.Classes.Services;
using eBRestarter.Views.Windows;
using System.Windows;
using System.Windows.Controls;

namespace eBRestarter.Views.UserControls
{

    public partial class UC_AdditionalOptions : UserControl
    {

        private readonly IEBXMLFile eBXMLFileManagement = new EBXMLFileManagement();

        public UC_AdditionalOptions()
        {
            InitializeComponent();
        }

        private void BtnInstalleBesucherAddOn_Click(object sender, RoutedEventArgs e)
        {
            _ = new InstalledeVisitorAddOnWindows().ShowDialog();
        }

        private void BtnDeleteBrowserContent_Click(object sender, RoutedEventArgs e)
        {
            BrowserDeleteProcessInterfaceService bPIService = new();

            if (MatchFirefoxBrowser() == true)
            {
                var dlb = new DeleteBrowserContentWindow(bPIService.ListContainer, bPIService.BrowserName!, bPIService.BrowserIconPath!, bPIService.FirefoxCookiePath!, false)
                {
                    GetFirefoxCookiePath = bPIService.FirefoxCookiePath!
                };

                dlb.ShowDialog();
            }
            else
            {
                _ = new DeleteBrowserContentWindow(bPIService.ListContainer, bPIService.BrowserName!, bPIService.BrowserIconPath!, "", false).ShowDialog();
            }
        }

        public bool MatchFirefoxBrowser()
        {
            EBXMLFileService eBFileService = new(eBXMLFileManagement);

            if (eBFileService.GetXMLAttributeInConfig<string>(ISystemPaths.File_Path_eBRestarter_Settings, IConfigConstants.BrowserTag, IConfigConstants.BrowserTag_Selected_Attribut).Equals("Firefox"))
            {
                return true;
            }
            else { return false; }
        }

        private void BtnInfoeVisitorAddOn_Click(object sender, RoutedEventArgs e)
        {
            EVisitorAddOnInformation eVisitorAddOnInformation = new();

            eVisitorAddOnInformation.ShowDialog();
        }
    }
}
