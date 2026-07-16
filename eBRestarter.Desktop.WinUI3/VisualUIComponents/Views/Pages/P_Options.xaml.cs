using Microsoft.UI.Xaml.Controls;

namespace eBRestarter.Desktop.WinUI3.VisualUIComponents.Views.Pages;

public sealed partial class P_Options : Page
{
    public P_Options()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // Überprüfe, ob ein Parameter übergeben wurde
        if (e.Parameter is int index)
        {
            UcOptions.SelectPivotIndex(index);
        }
        else if (e.Parameter is string param && int.TryParse(param, out int parsedIndex))
        {
            UcOptions.SelectPivotIndex(parsedIndex);
        }
    }
}
