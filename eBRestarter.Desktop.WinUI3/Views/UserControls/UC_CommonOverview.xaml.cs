using eBRestarter.Desktop.WinUI3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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
        System.ArgumentNullException.ThrowIfNull(sender);
        System.ArgumentNullException.ThrowIfNull(e);
    }
}
