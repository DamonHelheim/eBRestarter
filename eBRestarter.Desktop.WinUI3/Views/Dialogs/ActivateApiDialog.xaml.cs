using eBRestarter.Desktop.WinUI3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace eBRestarter.Desktop.WinUI3.Views.Dialogs;

public sealed partial class ActivateApiDialog : ContentDialog
{
    public ViewModelActivateApi ViewModelActivateApi { get; }

    public ActivateApiDialog()
    {
        InitializeComponent();

        ViewModelActivateApi = App.AppHost!.Services.GetRequiredService<ViewModelActivateApi>();

        this.DataContext = ViewModelActivateApi;
    }
}
