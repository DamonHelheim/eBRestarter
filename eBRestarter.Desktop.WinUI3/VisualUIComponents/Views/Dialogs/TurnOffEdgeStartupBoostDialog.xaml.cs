using eBRestarter.Desktop.WinUI3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace eBRestarter.Desktop.WinUI3.VisualUIComponents.Views.Dialogs;

public partial class TurnOffEdgeStartupBoostDialog : ContentDialog
{
    public ViewModelTurnOffEdgeStartupBoost ViewModel { get; }

    public TurnOffEdgeStartupBoostDialog()
    {
        // x:Bind wertet gegen das Code-Behind aus – das ViewModel muss VOR
        // InitializeComponent() gesetzt sein.
        ViewModel = App.AppHost!.Services.GetRequiredService<ViewModelTurnOffEdgeStartupBoost>();

        this.InitializeComponent();

        // ViewModel aus dem DI-Container holen

        this.DataContext = ViewModel;
    }
}
