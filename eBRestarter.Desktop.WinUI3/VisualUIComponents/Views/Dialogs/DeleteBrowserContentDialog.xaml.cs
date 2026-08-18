using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using eBRestarter.Desktop.WinUI3.ViewModels;

namespace eBRestarter.Desktop.WinUI3.VisualUIComponents.Views.Dialogs;

public sealed partial class DeleteBrowserContentDialog : ContentDialog
{
    // ═══════════════════════════════════════════════════════
    //  4. Properties
    // ═══════════════════════════════════════════════════════
    // ── Block 4: Komplexe Typen, Collections & UI-Elemente ──
    public ViewModelDeleteBrowserContent ViewModelDeleteBrowserContent { get; }


    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════
    public DeleteBrowserContentDialog()
    {
        // x:Bind wertet gegen das Code-Behind aus – das ViewModel muss VOR
        // InitializeComponent() gesetzt sein.
        ViewModelDeleteBrowserContent = App.AppHost!.Services.GetRequiredService<ViewModelDeleteBrowserContent>();

        InitializeComponent();

        DataContext = ViewModelDeleteBrowserContent;
        Loaded += DeleteBrowserContentDialog_Loaded;
        Unloaded += DeleteBrowserContentDialog_Unloaded;
    }


    // ═══════════════════════════════════════════════════════
    //  8. Methods (public → private)
    // ═══════════════════════════════════════════════════════
    private void CloseDialog() => Hide();

    private void DeleteBrowserContentDialog_Loaded(object? sender, RoutedEventArgs eventArgs) =>
        ViewModelDeleteBrowserContent.RequestCloseDialog += CloseDialog;

    private void DeleteBrowserContentDialog_Unloaded(object? sender, RoutedEventArgs eventArgs) =>
        ViewModelDeleteBrowserContent.RequestCloseDialog -= CloseDialog;
}
