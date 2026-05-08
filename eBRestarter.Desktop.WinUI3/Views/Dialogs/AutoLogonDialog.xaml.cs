using eBRestarter.Core.Application.Models.Records;
using Microsoft.UI.Xaml.Controls;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace eBRestarter.Desktop.WinUI3.Views.Dialogs
{
    // HIER üNDERN: Statt ": UserControl" muss hier ": ContentDialog" stehen
    public sealed partial class AutoLogonDialog : ContentDialog
    {
        public AutoLogonDialog()
        {
            this.InitializeComponent();
        }
        public void SetDefaults(string user, string domain)
        {
            if (!string.IsNullOrEmpty(user))
            {
                TxtUsername.Text = user;
            }

            if (!string.IsNullOrEmpty(domain))
            {
                TxtDomain.Text = domain;
            }
        }

        public AutoLogonCredentials GetCredentials()
        {
            return new AutoLogonCredentials(
                TxtUsername.Text,
                TxtDomain.Text,
                PbPassword.Password
            );
        }
    }
}
