using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace eBRestarter.Desktop.WinUI3.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class P_Options : Page
    {
        public P_Options()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            // Überprüfe, ob ein Parameter übergeben wurde
            if (e.Parameter is int index)
            {
                UcOptions.SelectPivotIndex(index);
            }
            else if (e.Parameter is string param && int.TryParse(param, out int parsedIndex))
            {
                UcOptions.SelectPivotIndex(parsedIndex);
            }
        }
    }
}
