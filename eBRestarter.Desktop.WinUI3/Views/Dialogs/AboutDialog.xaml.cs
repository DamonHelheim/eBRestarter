using eBRestarter.Desktop.WinUI3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace eBRestarter.Desktop.WinUI3.Views.Dialogs;

public sealed partial class AboutDialog : ContentDialog
{
    public ViewModelAbout ViewModelAbout { get; }

    public AboutDialog()
    {
        this.InitializeComponent();

        // HIER HOLST DU DAS VIEWMODEL
        // Entweder per Dependency Injection (App.Services...) oder manuell:

        // Manuell (Quick & Dirty zum Testen):
        // ViewModel = new AboutViewModel(new Infrastructure.Services.AppInfoService());

        // Besser via DI (wenn vorhanden):
        ViewModelAbout = App.AppHost!.Services.GetRequiredService<ViewModelAbout>();

        this.DataContext = ViewModelAbout;
    }
}
