using eBRestarter.Desktop.WinUI3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace eBRestarter.Desktop.WinUI3.Views.UserControls;

public sealed partial class UC_Options : UserControl
{
    public ViewModelOptions ViewModelOptions { get; }

    public UC_Options()
    {
        InitializeComponent();

        // Hier holen wir uns das ViewModel manuell aus dem Container
        ViewModelOptions = App.AppHost!.Services.GetRequiredService<ViewModelOptions>();

        // DataContext setzen
        this.DataContext = ViewModelOptions;
    }

}
