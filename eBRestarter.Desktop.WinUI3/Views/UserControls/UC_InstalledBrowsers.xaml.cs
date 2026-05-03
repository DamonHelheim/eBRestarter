using eBRestarter.Desktop.WinUI3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace eBRestarter.Desktop.WinUI3.Views.UserControls
{
    public sealed partial class UC_InstalledBrowsers : UserControl
    {
        public ViewModelInstalledBrowsers ViewModelInstalledBrowsers { get; }

        public UC_InstalledBrowsers()
        {
            InitializeComponent();

            // Hier holen wir uns das ViewModel manuell aus dem Container
            ViewModelInstalledBrowsers = App.AppHost!.Services.GetRequiredService<ViewModelInstalledBrowsers>();

            // DataContext setzen
            this.DataContext = ViewModelInstalledBrowsers;
        }
    }
}
