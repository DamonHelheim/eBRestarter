using eBRestarter.Desktop.WinUI3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace eBRestarter.Desktop.WinUI3.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// The WinUI Frame always invokes "new Page()".
    /// Therefore, pages must not accept parameters in their constructor.
    /// The solution is to expose the DI container (AppHost) statically and call GetRequiredService<T>() inside the page.
    /// </summary>
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
}