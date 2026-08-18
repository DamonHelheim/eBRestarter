using System;

using eBRestarter.Desktop.WinUI3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace eBRestarter.Desktop.WinUI3.VisualUIComponents.Views.Dialogs;

public sealed partial class InstallAddOnDialog : ContentDialog
{
    public ViewModelInstallAddOn ViewModelInstallAddOn { get; }

    public InstallAddOnDialog()
    {
        InitializeComponent();

        // ✅ Guide Kap. 22.4: Über die Fabrik erzeugt statt per GetRequiredService – so hält der
        // Root-Container keine Referenz auf dieses IDisposable-ViewModel. Der Dialog ist
        // alleiniger Besitzer und gibt es in Closed wieder frei.
        ViewModelInstallAddOn = App.AppHost!.Services.GetRequiredService<Func<ViewModelInstallAddOn>>()();

        DataContext = ViewModelInstallAddOn;

        Closed += (_, _) => ViewModelInstallAddOn.Dispose();
    }
}
