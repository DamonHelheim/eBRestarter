using eBRestarter.Core.Application.Models.Records;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace eBRestarter.Desktop.WinUI3.Views.Dialogs
{
    // HIER �NDERN: Statt ": UserControl" muss hier ": ContentDialog" stehen
    public sealed partial class AutoLogonDialog : ContentDialog
    {
        public AutoLogonDialog()
        {
            this.InitializeComponent();
        }

        // --- NEU HINZUF�GEN ---
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
        // ----------------------

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
