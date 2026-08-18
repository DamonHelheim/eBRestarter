using eBRestarter.Desktop.WinUI3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace eBRestarter.Desktop.WinUI3.VisualUIComponents.Views.Dialogs;

public sealed partial class ActivateApiDialog : ContentDialog
{
    public ViewModelActivateApi ViewModelActivateApi { get; }

    public ActivateApiDialog()
    {
        // x:Bind wertet gegen das Code-Behind aus – das ViewModel muss VOR
        // InitializeComponent() gesetzt sein.
        ViewModelActivateApi = App.AppHost!.Services.GetRequiredService<ViewModelActivateApi>();

        InitializeComponent();


        this.DataContext = ViewModelActivateApi;
    }
}
