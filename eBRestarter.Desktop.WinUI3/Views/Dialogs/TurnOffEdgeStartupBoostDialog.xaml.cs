using eBRestarter.Desktop.WinUI3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace eBRestarter.Desktop.WinUI3.Views.Dialogs;

public partial class TurnOffEdgeStartupBoostDialog : ContentDialog
{
    public ViewModelTurnOffEdgeStartupBoost ViewModel { get; }

    public TurnOffEdgeStartupBoostDialog()
    {
        this.InitializeComponent();

        // ViewModel aus dem DI-Container holen
        ViewModel = App.AppHost!.Services.GetRequiredService<ViewModelTurnOffEdgeStartupBoost>();

        this.DataContext = ViewModel;
    }
}
