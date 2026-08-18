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
        // x:Bind wertet gegen das Code-Behind aus – das ViewModel muss VOR
        // InitializeComponent() gesetzt sein.
        ViewModelRestarterProperties = App.AppHost!.Services.GetRequiredService<ViewModelRestarterProperties>();

        this.InitializeComponent();

        // Manually resolve the ViewModel from the container here

        // Set the DataContext
        this.DataContext = ViewModelRestarterProperties;
    }
}