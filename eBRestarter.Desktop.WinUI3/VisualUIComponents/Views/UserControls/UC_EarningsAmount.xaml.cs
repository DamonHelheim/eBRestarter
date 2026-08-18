using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace eBRestarter.Desktop.WinUI3.VisualUIComponents.Views.UserControls;

public sealed partial class UC_EarningsAmount : UserControl
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants / Static Readonly Members
    // ═══════════════════════════════════════════════════════
    // ── Block 4: Komplexe Typen, Collections & UI-Elemente ──
    public static readonly DependencyProperty EarningsAmountProperty =
        DependencyProperty.Register(
            nameof(EarningsAmount),
            typeof(string),
            typeof(UC_EarningsAmount),
            new PropertyMetadata(string.Empty));

    // ✅ Guide Kap. 23.1: Der x:Bind-Compiler hat die vorher stillschweigend zur Laufzeit
    // gecastete Zuweisung object -> ImageSource als Fehler gemeldet. Alle Zuweisungen sind
    // BitmapImage (siehe Light-/DarkTheme.xaml), der Typ wird daher korrekt eingeengt.
    public static readonly DependencyProperty HeaderImageEarningsProperty =
        DependencyProperty.Register(
            nameof(HeaderImageEarnings),
            typeof(ImageSource),
            typeof(UC_EarningsAmount),
            new PropertyMetadata(null));

    public static readonly DependencyProperty HeaderTitleEarningsProperty =
        DependencyProperty.Register(
            nameof(HeaderTitleEarnings),
            typeof(string),
            typeof(UC_EarningsAmount),
            new PropertyMetadata(string.Empty));


    // ═══════════════════════════════════════════════════════
    //  4. Properties
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitive Typen & Strings ──
    public string EarningsAmount
    {
        get => (string)GetValue(EarningsAmountProperty);
        set => SetValue(EarningsAmountProperty, value);
    }

    public string HeaderTitleEarnings
    {
        get => (string)GetValue(HeaderTitleEarningsProperty);
        set => SetValue(HeaderTitleEarningsProperty, value);
    }

    // ── Block 4: Komplexe Typen, Collections & UI-Elemente ──
    public ImageSource HeaderImageEarnings
    {
        get => (ImageSource)GetValue(HeaderImageEarningsProperty);
        set => SetValue(HeaderImageEarningsProperty, value);
    }


    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════
    public UC_EarningsAmount()
    {
        InitializeComponent();
    }
}
