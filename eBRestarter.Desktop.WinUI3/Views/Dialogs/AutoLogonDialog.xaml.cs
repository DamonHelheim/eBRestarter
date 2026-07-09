using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using Microsoft.UI.Xaml.Controls;

namespace eBRestarter.Desktop.WinUI3.Views.Dialogs;

public sealed partial class AutoLogonDialog : ContentDialog
{
    public AutoLogonDialog()
    {
        this.InitializeComponent();
    }
    public void SetDefaults(string user, string domain, bool isPasswordlessEnabled = false, bool isAdmin = false)
    {
        if (!string.IsNullOrEmpty(user))
        {
            TxtUsername.Text = user;
        }

        if (!string.IsNullOrEmpty(domain))
        {
            TxtDomain.Text = domain;
        }

        if (isPasswordlessEnabled)
        {
            if (!isAdmin)
            {
                AdminWarningInfoBar.IsOpen = true;
                IsPrimaryButtonEnabled = false;
            }
            else
            {
                CbDisablePasswordless.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                CbDisablePasswordless.IsChecked = true;
            }
        }
        else if (isAdmin)
        {
            CbRestorePasswordless.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        }
    }

    public bool DisablePasswordlessMode => CbDisablePasswordless.IsChecked ?? false;
    public bool RestorePasswordlessMode => CbRestorePasswordless.IsChecked ?? false;

    public AutoLogonCredentials GetCredentials()
    {
        return new AutoLogonCredentials(
            TxtUsername.Text,
            TxtDomain.Text,
            PbPassword.Password
        );
    }
}
