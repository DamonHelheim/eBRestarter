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

public sealed partial class UC_Options : UserControl
{
    public ViewModelOptions ViewModelOptions { get; }
    public UC_Options()
    {
        InitializeComponent();
        // Hier holen wir uns das ViewModel manuell aus dem Container
        ViewModelOptions = App.AppHost!.Services.GetRequiredService<ViewModelOptions>();

        // DataContext setzen
        this.DataContext = ViewModelOptions;
    }

    private void OptionsSelectorBar_SelectionChanged(SelectorBar sender,
    SelectorBarSelectionChangedEventArgs args)
    {
        if (sender.SelectedItem is SelectorBarItem item &&
            item.Tag is string tag)
        {
            if (tag == "General")
            {
                Grid_Options_General.Visibility = Visibility.Visible;
                Grid_API_Access.Visibility = Visibility.Collapsed;
            }
            else if (tag == "Api")
            {
                Grid_Options_General.Visibility = Visibility.Collapsed;
                Grid_API_Access.Visibility = Visibility.Visible;
            }
        }
    }

    private void BtnOptionsAutomaticWindowsUserLogin_Click(object sender, RoutedEventArgs e)
    {

    }

    private void ComboboxComputerRestartOptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

    }

    private void SliderRestartComputer_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {

    }

    private void Tbl_restart_information_MouseLeftButtonDown(object sender, TappedRoutedEventArgs e)
    {

    }

    private void BtnCheckForUpdates_Click(object sender, RoutedEventArgs e)
    {

    }

    private void Btn_go_to_settings_data_Click(object sender, RoutedEventArgs e)
    {

    }

    private void BtnSetStandardTheme_Click(object sender, RoutedEventArgs e)
    {

    }

    private void BtnSetDarkTheme_Click(object sender, RoutedEventArgs e)
    {

    }

    private void BtnShowAPIKey_Click(object sender, RoutedEventArgs e)
    {

    }

    private void BtnConnectWithAPIInterface_Click(object sender, RoutedEventArgs e)
    {

    }

    private void BtnImportAPIFile_Click(object sender, RoutedEventArgs e)
    {

    }

    private void BtnGoToSavedAPIFile_Click(object sender, RoutedEventArgs e)
    {

    }

    private void BtnRemoveAPIFile_Click(object sender, RoutedEventArgs e)
    {

    }

    private void OnKeyDownHandlerSliderRestartComputerTime(object sender, KeyRoutedEventArgs e)
    {

    }
}
