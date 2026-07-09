using Microsoft.UI.Xaml.Controls;

namespace eBRestarter.Desktop.WinUI3.Views.UserControls;

public sealed partial class UC_Options : UserControl
{
    public UC_Options()
    {
        InitializeComponent();
    }

    public void SelectPivotIndex(int index)
    {
        OptionsPivot.SelectedIndex = index;
    }
}
