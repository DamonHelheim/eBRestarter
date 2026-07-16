using eBRestarter.Desktop.WinUI3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace eBRestarter.Desktop.WinUI3.VisualUIComponents.Views.UserControls;

public sealed partial class UC_Infocenter : UserControl
{
    public ViewModelInfocenter ViewModelInfocenter { get; }

    public UC_Infocenter()
    {
        InitializeComponent();

        ViewModelInfocenter = App.AppHost!.Services.GetRequiredService<ViewModelInfocenter>();

        this.DataContext = ViewModelInfocenter;
    }
}
