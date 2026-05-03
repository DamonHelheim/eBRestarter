using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace eBRestarter.Desktop.WinUI3.Views.UserControls;

public sealed partial class UC_EarningsAmount : UserControl
{
    // HeaderTitleEarnings
    public static readonly DependencyProperty HeaderTitleEarningsProperty =
        DependencyProperty.Register(
            nameof(HeaderTitleEarnings),
            typeof(string),
            typeof(UC_EarningsAmount),
            new PropertyMetadata(string.Empty));

    public string HeaderTitleEarnings
    {
        get => (string)GetValue(HeaderTitleEarningsProperty);
        set => SetValue(HeaderTitleEarningsProperty, value);
    }

    // EarningsAmount
    public static readonly DependencyProperty EarningsAmountProperty =
        DependencyProperty.Register(
            nameof(EarningsAmount),
            typeof(string),
            typeof(UC_EarningsAmount),
            new PropertyMetadata(string.Empty));

    public string EarningsAmount
    {
        get => (string)GetValue(EarningsAmountProperty);
        set => SetValue(EarningsAmountProperty, value);
    }

    // HeaderImageEarnings (Image.Source erwartet ImageSource)
    public static readonly DependencyProperty HeaderImageEarningsProperty =
        DependencyProperty.Register(
            nameof(HeaderImageEarnings),
            typeof(object),
            typeof(UC_EarningsAmount),
            new PropertyMetadata(null));

    public object HeaderImageEarnings
    {
        get => (object)GetValue(HeaderImageEarningsProperty);
        set => SetValue(HeaderImageEarningsProperty, value);
    }

    public UC_EarningsAmount()
    {
        InitializeComponent();
        // KEIN manuelles DataContext-Setzen hier

        //tbl_sum_points_daily.DataContext = this;
        //headerImage.DataContext = this;
        //tbl_EarningsDay.DataContext = this;
    }
}

