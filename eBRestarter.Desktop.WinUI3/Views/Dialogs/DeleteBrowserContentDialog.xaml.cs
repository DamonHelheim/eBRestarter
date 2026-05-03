using eBRestarter.Desktop.WinUI3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace eBRestarter.Desktop.WinUI3.Views.Dialogs
{
    public partial class DeleteBrowserContentDialog : ContentDialog
    {
        public ViewModelDeleteBrowserContent ViewModelDeleteBrowserContent { get; }

        public DeleteBrowserContentDialog()
        {
            this.InitializeComponent();

            ViewModelDeleteBrowserContent = App.AppHost!.Services.GetRequiredService<ViewModelDeleteBrowserContent>();
            this.DataContext = ViewModelDeleteBrowserContent;

            // NEU: Event abonnieren beim Laden
            this.Loaded += DeleteBrowserContentDialog_Loaded;
            this.Unloaded += DeleteBrowserContentDialog_Unloaded;
        }

        private void DeleteBrowserContentDialog_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Wenn das ViewModel "Schlieﬂen" sagt, schlieﬂen wir den Dialog
            ViewModelDeleteBrowserContent.RequestCloseDialog += CloseDialog;
        }

        private void DeleteBrowserContentDialog_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Sauber aufr‰umen
            ViewModelDeleteBrowserContent.RequestCloseDialog -= CloseDialog;
        }

        private void CloseDialog()
        {
            // Hide() schlieﬂt einen ContentDialog in WinUI 3
            this.Hide();
        }
    }
}
