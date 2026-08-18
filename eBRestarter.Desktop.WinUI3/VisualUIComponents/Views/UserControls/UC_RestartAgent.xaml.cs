using eBRestarter.Desktop.WinUI3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace eBRestarter.Desktop.WinUI3.VisualUIComponents.Views.UserControls;

public sealed partial class UC_RestartAgent : UserControl
{
    public ViewModelRestartTask ViewModelRestartTask { get; }

    public UC_RestartAgent()
    {
        // x:Bind wertet gegen das Code-Behind aus – das ViewModel muss VOR
        // InitializeComponent() gesetzt sein.
        ViewModelRestartTask = App.AppHost!.Services.GetRequiredService<ViewModelRestartTask>();

        InitializeComponent();


        this.DataContext = ViewModelRestartTask;
    }
}
