using eBRestarter.Desktop.WinUI3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace eBRestarter.Desktop.WinUI3.VisualUIComponents.Views.UserControls;

public sealed partial class UC_OptionsGeneral : UserControl
{
    public ViewModelOptionsGeneral ViewModel { get; }

    public UC_OptionsGeneral()
    {
        InitializeComponent();
        ViewModel = App.AppHost!.Services.GetRequiredService<ViewModelOptionsGeneral>();
        this.DataContext = ViewModel;
    }
}
