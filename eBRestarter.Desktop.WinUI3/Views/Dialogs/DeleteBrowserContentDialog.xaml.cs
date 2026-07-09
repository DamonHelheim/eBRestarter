using eBRestarter.Desktop.WinUI3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace eBRestarter.Desktop.WinUI3.Views.Dialogs;

public partial class DeleteBrowserContentDialog : ContentDialog
{
    public ViewModelDeleteBrowserContent ViewModelDeleteBrowserContent { get; }

    public DeleteBrowserContentDialog()
    {
        this.InitializeComponent();

        ViewModelDeleteBrowserContent = App.AppHost!.Services.GetRequiredService<ViewModelDeleteBrowserContent>();
        this.DataContext = ViewModelDeleteBrowserContent;
        this.Loaded += DeleteBrowserContentDialog_Loaded;
        this.Unloaded += DeleteBrowserContentDialog_Unloaded;
    }

    private void DeleteBrowserContentDialog_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ViewModelDeleteBrowserContent.RequestCloseDialog += CloseDialog;
    }

    private void DeleteBrowserContentDialog_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ViewModelDeleteBrowserContent.RequestCloseDialog -= CloseDialog;
    }

    private void CloseDialog()
    {
        this.Hide();
    }
}
