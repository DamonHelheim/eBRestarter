using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace eBRestarter.Desktop.WinUI3.VisualUIComponents.Views.UserControls;

public sealed partial class UC_InformationsCard : UserControl
{
    // HEADER / TITEL
    public static readonly DependencyProperty HeaderTitleProperty =
        DependencyProperty.Register(
            nameof(HeaderTitle),
            typeof(string),
            typeof(UC_InformationsCard),
            new PropertyMetadata(string.Empty));

    public string HeaderTitle
    {
        get => (string)GetValue(HeaderTitleProperty);
        set => SetValue(HeaderTitleProperty, value);
    }

    public static readonly DependencyProperty TitleForegroundProperty =
        DependencyProperty.Register(
            nameof(TitleForeground),
            typeof(Brush),
            typeof(UC_InformationsCard),
            new PropertyMetadata(null));

    public Brush TitleForeground
    {
        get => (Brush)GetValue(TitleForegroundProperty);
        set => SetValue(TitleForegroundProperty, value);
    }

    // HEADER BILD
    public static readonly DependencyProperty HeaderImageProperty =
        DependencyProperty.Register(
            nameof(HeaderImage),
            typeof(ImageSource),
            typeof(UC_InformationsCard),
            new PropertyMetadata(null));

    public ImageSource HeaderImage
    {
        get => (ImageSource)GetValue(HeaderImageProperty);
        set => SetValue(HeaderImageProperty, value);
    }

    // HINTERGRUND
    public static readonly DependencyProperty BackgroundBrushProperty =
        DependencyProperty.Register(
            nameof(BackgroundBrush),
            typeof(Brush),
            typeof(UC_InformationsCard),
            new PropertyMetadata(null));

    public Brush BackgroundBrush
    {
        get => (Brush)GetValue(BackgroundBrushProperty);
        set => SetValue(BackgroundBrushProperty, value);
    }

    // BILDGR�SSE
    public static readonly DependencyProperty ImageHeightProperty =
        DependencyProperty.Register(
            nameof(ImageHeight),
            typeof(double),
            typeof(UC_InformationsCard),
            new PropertyMetadata(24.0));

    public double ImageHeight
    {
        get => (double)GetValue(ImageHeightProperty);
        set => SetValue(ImageHeightProperty, value);
    }

    public static readonly DependencyProperty ImageWidthProperty =
        DependencyProperty.Register(
            nameof(ImageWidth),
            typeof(double),
            typeof(UC_InformationsCard),
            new PropertyMetadata(24.0));

    public double ImageWidth
    {
        get => (double)GetValue(ImageWidthProperty);
        set => SetValue(ImageWidthProperty, value);
    }

    // INFORMATION ROWS
    public static readonly DependencyProperty InformationRow1Property =
        DependencyProperty.Register(
            nameof(InformationRow1),
            typeof(string),
            typeof(UC_InformationsCard),
            new PropertyMetadata(string.Empty));

    public string InformationRow1
    {
        get => (string)GetValue(InformationRow1Property);
        set => SetValue(InformationRow1Property, value);
    }

    public static readonly DependencyProperty InformationRow2Property =
        DependencyProperty.Register(
            nameof(InformationRow2),
            typeof(string),
            typeof(UC_InformationsCard),
            new PropertyMetadata(string.Empty));

    public string InformationRow2
    {
        get => (string)GetValue(InformationRow2Property);
        set => SetValue(InformationRow2Property, value);
    }

    public static readonly DependencyProperty InformationRow3Property =
        DependencyProperty.Register(
            nameof(InformationRow3),
            typeof(string),
            typeof(UC_InformationsCard),
            new PropertyMetadata(string.Empty));

    public string InformationRow3
    {
        get => (string)GetValue(InformationRow3Property);
        set => SetValue(InformationRow3Property, value);
    }

    public static readonly DependencyProperty InformationRowForegroundProperty =
        DependencyProperty.Register(
            nameof(InformationRowForeground),
            typeof(Brush),
            typeof(UC_InformationsCard),
            new PropertyMetadata(null));

    public Brush InformationRowForeground
    {
        get => (Brush)GetValue(InformationRowForegroundProperty);
        set => SetValue(InformationRowForegroundProperty, value);
    }

    public static readonly DependencyProperty BackGroundColorProperty =
        DependencyProperty.Register(
            nameof(BackGroundColor),
            typeof(Brush),
            typeof(UC_InformationsCard),
            new PropertyMetadata(null));

    public Brush BackGroundColor
    {
        get => (Brush)GetValue(BackGroundColorProperty);
        set => SetValue(BackGroundColorProperty, value);
    }

    public UC_InformationsCard()
    {
        InitializeComponent();
    }
}
