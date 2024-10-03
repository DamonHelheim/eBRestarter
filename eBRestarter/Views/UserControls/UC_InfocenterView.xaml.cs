using eBRestarter.Classes.InternetBrowser;
using eBRestarter.Classes.OperatingSystem;
using eBRestarter.Views.Windows;
using System.Windows.Controls;
using eBRestarter.ViewModel.ViewModels;

namespace eBRestarter.Views.UserControls
{

    public partial class UC_InfocenterView : UserControl, IWebLinks
    {
        public InfocenterViewModel InfocenterViewModel { get; } = new();

        public UC_InfocenterView(InfocenterViewModel infocenterViewModel)
        {
            InitializeComponent();

            InfocenterViewModel = infocenterViewModel;

            DataContext = InfocenterViewModel;
        }

        private void Btn_about_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AbouteBRestarter abouteBRestarter = new();

            abouteBRestarter.ShowDialog();
        }

        private void Btn_register_with_reflink_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            WindowsOS windowsOS = new();

            windowsOS.OpenLinkWithStandardbrowser(IWebLinks.REGISTRATION_LINK);
        }
    }
}
