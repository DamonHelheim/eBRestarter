using eBRestarter.Classes.InternetBrowser;
using System.Windows.Controls;
using eBRestarter.ViewModel.ViewModels;

namespace eBRestarter.Views.UserControls
{

    public partial class UC_InstalledBrowsers : UserControl
    {
        public const string DownloadLinkFirefox = IWebLinks.FIREFOX_DOWNLOAD_LINK;
        public const string DownloadLinkChrome = IWebLinks.CHROME_DOWNLOAD_LINK_DE;
        public const string DownloadLinkEdge = IWebLinks.EDGE_DOWNLOAD_LINK_DE;

        public BrowserViewModel? BrowserViewModel { get; } = new(); //Must be initialized beforehand, otherwise no value appears

        public UC_InstalledBrowsers()
        {
            InitializeComponent();
           
            DataContext = BrowserViewModel;

        }
    }
}
