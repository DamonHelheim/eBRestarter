using eBRestarter.Desktop.WinUI3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace eBRestarter.Desktop.WinUI3.Views.Dialogs;

public sealed partial class InstallAddOnDialog : ContentDialog
{
    public ViewModelInstallAddOn ViewModelInstallAddOn { get; }

    public InstallAddOnDialog()
    {
        InitializeComponent();
        // ViewModel via DI holen
        ViewModelInstallAddOn = App.AppHost!.Services.GetRequiredService<ViewModelInstallAddOn>();

        this.DataContext = ViewModelInstallAddOn;

        // Wenn Dialog geschlossen wird, Timer stoppen
        this.Closed += (_, __) => ViewModelInstallAddOn.Dispose();
    }
}
