using eBRestarter.Desktop.WinUI3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace eBRestarter.Desktop.WinUI3.VisualUIComponents.Views.UserControls;

public sealed partial class UC_OptionsApi : UserControl
{
    public ViewModelOptionsApi ViewModel { get; }

    public UC_OptionsApi()
    {
        // x:Bind wertet gegen das Code-Behind aus – das ViewModel muss deshalb VOR
        // InitializeComponent() gesetzt sein.
        ViewModel = App.AppHost!.Services.GetRequiredService<ViewModelOptionsApi>();

        InitializeComponent();

        this.DataContext = ViewModel;
    }
}
