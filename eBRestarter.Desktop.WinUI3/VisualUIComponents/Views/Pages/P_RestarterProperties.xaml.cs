using eBRestarter.Desktop.WinUI3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace eBRestarter.Desktop.WinUI3.VisualUIComponents.Views.Pages;

public sealed partial class P_RestarterProperties : Page
{
    // No "new()" here, because we want to retrieve it from the DI container!
    public ViewModelRestarterProperties ViewModelRestarterProperties { get; }

    // IMPORTANT: The constructor must be EMPTY (parameterless)
    public P_RestarterProperties()
    {
        this.InitializeComponent();

        // Manually resolve the ViewModel from the container here
        ViewModelRestarterProperties = App.AppHost!.Services.GetRequiredService<ViewModelRestarterProperties>();

        // Set the DataContext
        this.DataContext = ViewModelRestarterProperties;
    }
}