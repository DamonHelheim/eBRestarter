using eBRestarter.Desktop.WinUI3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace eBRestarter.Desktop.WinUI3.VisualUIComponents.Views.UserControls
{
    public sealed partial class UC_InstalledBrowsers : UserControl
    {
        public ViewModelInstalledBrowsers ViewModelInstalledBrowsers { get; }

        public UC_InstalledBrowsers()
        {
            InitializeComponent();

            // Manually resolve the ViewModel from the container here
            ViewModelInstalledBrowsers = App.AppHost!.Services.GetRequiredService<ViewModelInstalledBrowsers>();

            // Set the DataContext
            this.DataContext = ViewModelInstalledBrowsers;
        }
    }
}