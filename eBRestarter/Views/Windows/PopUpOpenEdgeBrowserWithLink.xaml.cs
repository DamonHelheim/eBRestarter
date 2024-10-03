using eBRestarter.Classes.InternetBrowser;
using MahApps.Metro.Controls;
using System.Windows;

namespace eBRestarter.Views.Windows
{
    public partial class PopUpOpenEdgeBrowserWithLink : MetroWindow
    {
        private readonly Browser edgeBrowser = new Edge();

        public PopUpOpenEdgeBrowserWithLink()
        {
            InitializeComponent();
        }

        private void BtnCopyLink_Click(object sender, RoutedEventArgs e)
        {
            if (edgeBrowser.BrowserVersion != null && edgeBrowser.BrowserExist is true)
            {
                Clipboard.SetText("edge://settings/?search=Startup-Boost");

                string message = "Folgender Link wurde jetzt kopiert: edge://settings/?search=Startup-Boost" + '\n' + '\n' + 
                                 "Sobald du auf OK klickst, öffnet sich der Edgebrowser und du kannst den Link dort einfügen.";

                string title = "Edgebrowser";

                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

                edgeBrowser.StartBrowser();
            }
            else
            {
                string message = "Der Edgebrwoser ist nicht installiert.";
                string title = "Edgebrowser";

                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
