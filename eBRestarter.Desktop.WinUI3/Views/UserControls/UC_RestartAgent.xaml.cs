using eBRestarter.Desktop.WinUI3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace eBRestarter.Desktop.WinUI3.Views.UserControls;

public sealed partial class UC_RestartAgent : UserControl
{
    public ViewModelRestartTask ViewModelRestartTask { get; }

    public UC_RestartAgent()
    {
        InitializeComponent();

        ViewModelRestartTask = App.AppHost!.Services.GetRequiredService<ViewModelRestartTask>();

        this.DataContext = ViewModelRestartTask;
    }
}