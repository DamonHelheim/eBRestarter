using eBRestarter.Desktop.WinUI3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace eBRestarter.Desktop.WinUI3.VisualUIComponents.Views.Dialogs;

public sealed partial class InstallAddOnDialog : ContentDialog
{
    public ViewModelInstallAddOn ViewModelInstallAddOn { get; }

    public InstallAddOnDialog()
    {
        InitializeComponent();

        ViewModelInstallAddOn = App.AppHost!.Services.GetRequiredService<ViewModelInstallAddOn>();
        
        DataContext = ViewModelInstallAddOn;

        Closed += (_, _) => ViewModelInstallAddOn.Dispose();
    }
}
