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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace eBRestarter.Desktop.WinUI3.Views.UserControls;

public sealed partial class UC_CommonOverview : UserControl
{
    public ViewModelGeneralOverview ViewModelGeneralOverView { get; }
    public UC_CommonOverview()
    {
        InitializeComponent();

        ViewModelGeneralOverView = App.AppHost!.Services.GetRequiredService<ViewModelGeneralOverview>();

        this.DataContext = ViewModelGeneralOverView;
    }

    private void BtnGoToeVisitorAPI_Click(object sender, RoutedEventArgs e)
    {

    }
}
