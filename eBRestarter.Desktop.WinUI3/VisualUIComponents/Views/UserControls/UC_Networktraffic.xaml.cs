using System;

using eBRestarter.Desktop.WinUI3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace eBRestarter.Desktop.WinUI3.VisualUIComponents.Views.UserControls;

public sealed partial class UC_Networktraffic : UserControl
{
    public ViewModelNetworkTraffic ViewModelNetworkTraffic { get; }

    public UC_Networktraffic()
    {
        InitializeComponent();

        // ✅ Guide Kap. 22.4: Über die Fabrik erzeugt – der Root-Container hält damit keine
        // Referenz mehr auf dieses IDisposable-ViewModel.
        ViewModelNetworkTraffic = App.AppHost!.Services.GetRequiredService<Func<ViewModelNetworkTraffic>>()();

        this.DataContext = ViewModelNetworkTraffic;

        // ✅ Guide Kap. 22.6: Der 1-Sekunden-Poll lief bisher ab App-Start durchgehend weiter.
        // Die Host-Page ist NavigationCacheMode="Required", überlebt also die Navigation – daher
        // wird hier pausiert statt disposed, sonst bliebe die Anzeige nach der Rückkehr tot.
        Loaded += (_, _) => ViewModelNetworkTraffic.StartPolling();
        Unloaded += (_, _) => ViewModelNetworkTraffic.StopPolling();
    }
}

