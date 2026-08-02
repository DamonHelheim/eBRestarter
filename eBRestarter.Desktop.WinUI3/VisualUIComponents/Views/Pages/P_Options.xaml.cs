using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace eBRestarter.Desktop.WinUI3.VisualUIComponents.Views.Pages;

public sealed partial class P_Options : Page
{
    public P_Options() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is int index || (e.Parameter is string param && int.TryParse(param, out index)))
        {
            UcOptions.SelectPivotIndex(index);
        }
    }
}
