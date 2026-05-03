using eBRestarter.Desktop.WinUI3.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace eBRestarter.Desktop.WinUI3.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// WinUI Frame ruft immer new Page() auf.
    /// Deshalb dürfen Pages keine Parameter im Konstruktor haben.
    /// Die Lösung ist, den DI-Container (AppHost) statisch verfügbar zu machen und in der Page GetRequiredService<T>() aufzurufen.
    /// </summary>
    public sealed partial class P_RestarterProperties : Page
    {
        // Kein "new()", da wir es aus dem DI Container wollen!
        public ViewModelRestarterProperties ViewModelRestarterProperties { get; }

        // WICHTIG: Der Konstruktor muss LEER sein (Parameterlos)
        public P_RestarterProperties()
        {
            this.InitializeComponent();

            // Hier holen wir uns das ViewModel manuell aus dem Container
            ViewModelRestarterProperties = App.AppHost!.Services.GetRequiredService<ViewModelRestarterProperties>();

            // DataContext setzen
            this.DataContext = ViewModelRestarterProperties;
        }
    }
}
