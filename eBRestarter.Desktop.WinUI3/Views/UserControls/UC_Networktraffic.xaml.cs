using eBRestarter.Desktop.WinUI3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace eBRestarter.Desktop.WinUI3.Views.UserControls;

public sealed partial class UC_Networktraffic : UserControl
{
    public ViewModelNetworkTraffic ViewModelNetworkTraffic { get; }

    public UC_Networktraffic()
    {
        InitializeComponent();

        // Dummy-Initialisierung (In echter App via DI Container auflüsen!)
        ViewModelNetworkTraffic = App.AppHost!.Services.GetRequiredService<ViewModelNetworkTraffic>();

        //Fallback für Design - Time:
        this.DataContext = ViewModelNetworkTraffic;
    }
}

