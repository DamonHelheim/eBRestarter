using eBRestarter.Desktop.WinUI3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

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
