using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Domain.Extensions;
using eBRestarter.Desktop.WinUI3.Models.UI;
using eBRestarter.Desktop.WinUI3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace eBRestarter.Desktop.WinUI3.Views.UserControls;

public sealed partial class UC_Networktraffic : UserControl
{
    public NetworkTrafficViewModel NetworkTrafficViewModel { get; }

    public UC_Networktraffic()
    {
        InitializeComponent();

        // Dummy-Initialisierung (In echter App via DI Container auflösen!)
        NetworkTrafficViewModel = App.AppHost!.Services.GetRequiredService<NetworkTrafficViewModel>();
        //Fallback für Design - Time:
        this.DataContext = NetworkTrafficViewModel;
    }
}

